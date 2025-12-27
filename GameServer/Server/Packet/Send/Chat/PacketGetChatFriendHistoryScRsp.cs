using EggLink.DanhengServer.Database.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Chat;

public class PacketGetChatFriendHistoryScRsp : BasePacket
{
    public PacketGetChatFriendHistoryScRsp(Dictionary<int, FriendChatHistory> history)
        : base(CmdIds.GetChatFriendHistoryScRsp)
    {
        var proto = new GetChatFriendHistoryScRsp();

        foreach (var item in history)
            proto.FriendHistoryInfo.Add(new FriendHistoryInfo
            {
                ContactSide = (uint)item.Key,
                LastSendTime = item.Value.MessageList.Last().SendTime
            });

        SetData(proto);
    }
}