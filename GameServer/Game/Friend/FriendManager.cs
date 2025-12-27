using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Friend;
using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.GameServer.Command;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Chat;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Friend;

public class FriendManager(PlayerInstance player) : BasePlayerManager(player)
{
    public FriendData FriendData { get; set; } =
        DatabaseHelper.Instance!.GetInstanceOrCreateNew<FriendData>(player.Uid);

    public async ValueTask<Retcode> AddFriend(int targetUid)
    {
        if (targetUid == Player.Uid) return Retcode.RetSucc; // Cannot add self
        if (FriendData.FriendDetailList.ContainsKey(targetUid)) return Retcode.RetFriendAlreadyIsFriend;
        if (FriendData.BlackList.Contains(targetUid)) return Retcode.RetFriendInBlacklist;
        if (FriendData.SendApplyList.Contains(targetUid)) return Retcode.RetSucc; // Already send apply

        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        if (target == null) return Retcode.RetFriendPlayerNotFound;
        if (target.BlackList.Contains(Player.Uid)) return Retcode.RetFriendInTargetBlacklist;
        if (target.ReceiveApplyList.Contains(targetUid)) return Retcode.RetSucc; // Already receive apply

        FriendData.SendApplyList.Add(targetUid);
        target.ReceiveApplyList.Add(Player.Uid);

        var targetPlayer = Listener.GetActiveConnection(targetUid);
        if (targetPlayer != null)
            await targetPlayer.SendPacket(new PacketSyncApplyFriendScNotify(Player.Data));

        DatabaseHelper.ToSaveUidList.Add(targetUid);
        return Retcode.RetSucc;
    }

    public async ValueTask<PlayerData?> ConfirmAddFriend(int targetUid)
    {
        if (targetUid == Player.Uid) return null; // Cannot add self
        if (FriendData.FriendDetailList.ContainsKey(targetUid)) return null;
        if (FriendData.BlackList.Contains(targetUid)) return null;

        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        var targetData = PlayerData.GetPlayerByUid(targetUid);
        if (target == null || targetData == null) return null;
        if (target.FriendDetailList.ContainsKey(Player.Uid)) return null;
        if (target.BlackList.Contains(Player.Uid)) return null;

        FriendData.ReceiveApplyList.Remove(targetUid);
        FriendData.FriendDetailList.Add(targetUid, new FriendDetailData());
        target.SendApplyList.Remove(Player.Uid);
        target.FriendDetailList.Add(Player.Uid, new FriendDetailData());

        var targetPlayer = Listener.GetActiveConnection(targetUid);
        if (targetPlayer != null)
            await targetPlayer.SendPacket(new PacketSyncHandleFriendScNotify((uint)Player.Uid, true, Player.Data));

        DatabaseHelper.ToSaveUidList.Add(targetUid);
        return targetData;
    }

    public async ValueTask RefuseAddFriend(int targetUid)
    {
        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        if (target == null) return;

        FriendData.ReceiveApplyList.Remove(targetUid);
        target.SendApplyList.Remove(Player.Uid);

        var targetPlayer = Listener.GetActiveConnection(targetUid);
        if (targetPlayer != null)
            await targetPlayer.SendPacket(new PacketSyncHandleFriendScNotify((uint)Player.Uid, false, Player.Data));

        DatabaseHelper.ToSaveUidList.Add(targetUid);
    }

    public async ValueTask<PlayerData?> AddBlackList(int targetUid)
    {
        var blackInfo = GetFriendPlayerData([targetUid]).First();
        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        if (blackInfo == null || target == null) return null;

        FriendData.FriendDetailList.Remove(targetUid);
        target.FriendDetailList.Remove(Player.Uid);
        if (!FriendData.BlackList.Contains(targetUid))
            FriendData.BlackList.Add(targetUid);

        var targetPlayer = Listener.GetActiveConnection(targetUid);
        if (targetPlayer != null)
            await targetPlayer.SendPacket(new PacketSyncAddBlacklistScNotify(Player.Uid));

        DatabaseHelper.ToSaveUidList.Add(targetUid);
        return blackInfo;
    }

    public void RemoveBlackList(int targetUid)
    {
        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        if (target == null) return;
        FriendData.BlackList.Remove(targetUid);
    }

    public async ValueTask<int?> RemoveFriend(int targetUid)
    {
        var target = DatabaseHelper.Instance!.GetInstance<FriendData>(targetUid);
        if (target == null) return null;

        FriendData.FriendDetailList.Remove(targetUid);
        target.FriendDetailList.Remove(Player.Uid);

        var targetPlayer = Listener.GetActiveConnection(targetUid);
        if (targetPlayer != null)
            await targetPlayer.SendPacket(new PacketSyncDeleteFriendScNotify(Player.Uid));

        DatabaseHelper.ToSaveUidList.Add(targetUid);
        return targetUid;
    }

    public async ValueTask SendMessage(int sendUid, int recvUid, string? message = null, int? extraId = null)
    {
        var data = new FriendChatData
        {
            SendUid = sendUid,
            ReceiveUid = recvUid,
            Message = message ?? "",
            ExtraId = extraId ?? 0,
            SendTime = Extensions.GetUnixSec()
        };

        if (!FriendData.ChatHistory.TryGetValue(recvUid, out var value))
        {
            FriendData.ChatHistory[recvUid] = new FriendChatHistory();
            value = FriendData.ChatHistory[recvUid];
        }

        value.MessageList.Add(data);

        PacketRevcMsgScNotify proto;
        if (message != null)
            proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, message);
        else
            proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, (uint)(extraId ?? 0));

        await Player.SendPacket(proto);

        if (recvUid == ConfigManager.Config.ServerOption.ServerProfile.Uid)
            if (message?.StartsWith('/') == true)
            {
                var cmd = message[1..];
                CommandExecutor.ExecuteCommand(new PlayerCommandSender(Player), cmd);
            }

        // receive message
        var recvPlayer = Listener.GetActiveConnection(recvUid)?.Player;
        if (recvPlayer != null)
        {
            await recvPlayer.FriendManager!.ReceiveMessage(sendUid, recvUid, message, extraId);
        }
        else
        {
            // offline
            var friendData = DatabaseHelper.Instance!.GetInstance<FriendData>(recvUid);
            if (friendData == null) return; // not exist maybe server profile
            if (!friendData.ChatHistory.TryGetValue(sendUid, out var history))
            {
                friendData.ChatHistory[sendUid] = new FriendChatHistory();
                history = friendData.ChatHistory[sendUid];
            }

            history.MessageList.Add(data);

            DatabaseHelper.ToSaveUidList.Add(recvUid);
        }
    }

    public async ValueTask SendInviteMessage(int sendUid, int recvUid, LobbyInviteInfo info)
    {
        var proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, info);
        await Player.SendPacket(proto);

        // receive message
        var recvPlayer = Listener.GetActiveConnection(recvUid)?.Player;
        if (recvPlayer != null) await recvPlayer.FriendManager!.ReceiveInviteMessage(sendUid, recvUid, info);
    }

    public async ValueTask ReceiveMessage(int sendUid, int recvUid, string? message = null, int? extraId = null)
    {
        var data = new FriendChatData
        {
            SendUid = sendUid,
            ReceiveUid = recvUid,
            Message = message ?? "",
            ExtraId = extraId ?? 0,
            SendTime = Extensions.GetUnixSec()
        };

        if (!FriendData.ChatHistory.TryGetValue(sendUid, out var value))
        {
            FriendData.ChatHistory[sendUid] = new FriendChatHistory();
            value = FriendData.ChatHistory[sendUid];
        }

        value.MessageList.Add(data);

        PacketRevcMsgScNotify proto;
        if (message != null)
            proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, message);
        else
            proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, (uint)(extraId ?? 0));

        await Player.SendPacket(proto);
    }

    public async ValueTask ReceiveInviteMessage(int sendUid, int recvUid, LobbyInviteInfo info)
    {
        var proto = new PacketRevcMsgScNotify((uint)recvUid, (uint)sendUid, info);

        await Player.SendPacket(proto);
    }

    public FriendDetailData? GetFriendDetailData(int uid)
    {
        if (uid == ConfigManager.Config.ServerOption.ServerProfile.Uid)
            return new FriendDetailData { IsMark = true };

        if (!FriendData.FriendDetailList.TryGetValue(uid, out var friend)) return null;

        return friend;
    }

    public List<ChatMessageData> GetHistoryInfo(int uid)
    {
        if (!FriendData.ChatHistory.TryGetValue(uid, out var history)) return [];

        var info = new List<ChatMessageData>();

        foreach (var chat in history.MessageList)
            info.Add(new ChatMessageData
            {
                CreateTime = (ulong)chat.SendTime,
                Content = chat.Message,
                ExtraId = (uint)chat.ExtraId,
                SenderId = (uint)chat.SendUid,
                MessageType = chat.ExtraId > 0 ? MsgType.Emoji : MsgType.CustomText
            });

        info.Reverse();

        return info;
    }

    public List<PlayerData> GetFriendPlayerData(List<int>? uids = null)
    {
        var list = new List<PlayerData>();
        uids ??= [.. FriendData.FriendDetailList.Keys];

        foreach (var friend in uids)
        {
            var player = PlayerData.GetPlayerByUid(friend);
            if (player != null) list.Add(player);
        }

        var serverProfile = ConfigManager.Config.ServerOption.ServerProfile;
        list.Add(new PlayerData
        {
            Uid = serverProfile.Uid,
            HeadIcon = serverProfile.HeadIcon,
            Signature = serverProfile.Signature,
            Level = serverProfile.Level,
            WorldLevel = 0,
            Name = serverProfile.Name,
            ChatBubble = serverProfile.ChatBubbleId,
            PersonalCard = serverProfile.PersonalCardId
        });

        return list;
    }

    public List<PlayerData> GetBlackList()
    {
        List<PlayerData> list = [];

        foreach (var friend in FriendData.BlackList)
        {
            var player = PlayerData.GetPlayerByUid(friend);

            if (player != null) list.Add(player);
        }

        return list;
    }

    public List<PlayerData> GetSendApplyList()
    {
        List<PlayerData> list = [];

        foreach (var friend in FriendData.SendApplyList)
        {
            var player = PlayerData.GetPlayerByUid(friend);

            if (player != null) list.Add(player);
        }

        return list;
    }

    public List<PlayerData> GetReceiveApplyList()
    {
        List<PlayerData> list = [];

        foreach (var friend in FriendData.ReceiveApplyList)
        {
            var player = PlayerData.GetPlayerByUid(friend);

            if (player != null) list.Add(player);
        }

        return list;
    }

    public List<PlayerData> GetRandomFriend()
    {
        var list = new List<PlayerData>();

        foreach (var kcp in DanhengListener.Connections.Values)
        {
            if (kcp.State != SessionStateEnum.ACTIVE) continue;
            if (kcp is not Connection connection) continue;
            if (connection.Player?.Uid == Player.Uid) continue;
            var data = connection.Player?.Data;
            if (data == null) continue;
            list.Add(data);
        }

        return list.Take(20).ToList();
    }

    public void RemarkFriendName(int uid, string remarkName)
    {
        if (!FriendData.FriendDetailList.TryGetValue(uid, out var friend)) return;
        friend.RemarkName = remarkName;
    }

    public void MarkFriend(int uid, bool isMark)
    {
        if (!FriendData.FriendDetailList.TryGetValue(uid, out var friend)) return;
        friend.IsMark = isMark;
    }

    public GetFriendListInfoScRsp ToProto()
    {
        var proto = new GetFriendListInfoScRsp();

        foreach (var player in GetFriendPlayerData())
        {
            var status = Listener.GetActiveConnection(player.Uid) == null
                ? FriendOnlineStatus.Offline
                : FriendOnlineStatus.Online;
            var friend = GetFriendDetailData(player.Uid) ?? new FriendDetailData();

            proto.FriendList.Add(new FriendSimpleInfo
            {
                PlayerInfo = player.ToSimpleProto(status),
                IsMarked = friend.IsMark,
                RemarkName = friend.RemarkName
            });
        }

        foreach (var player in GetBlackList())
        {
            var status = Listener.GetActiveConnection(player.Uid) == null
                ? FriendOnlineStatus.Offline
                : FriendOnlineStatus.Online;
            proto.BlackList.Add(player.ToSimpleProto(status));
        }

        return proto;
    }

    public GetFriendApplyListInfoScRsp ToApplyListProto()
    {
        GetFriendApplyListInfoScRsp proto = new();

        foreach (var player in GetSendApplyList()) proto.SendApplyList.Add((uint)player.Uid);

        foreach (var player in GetReceiveApplyList())
        {
            var status = Listener.GetActiveConnection(player.Uid) == null
                ? FriendOnlineStatus.Offline
                : FriendOnlineStatus.Online;
            proto.ReceiveApplyList.Add(new FriendApplyInfo
            {
                PlayerInfo = player.ToSimpleProto(status)
            });
        }

        return proto;
    }
}