using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketGetFriendRecommendListInfoScRsp : BasePacket
{
    public PacketGetFriendRecommendListInfoScRsp(List<PlayerData> friends)
        : base(CmdIds.GetFriendRecommendListInfoScRsp)
    {
        var proto = new GetFriendRecommendListInfoScRsp
        {
            PlayerInfoList =
            {
                friends.Select(x => new FriendRecommendInfo
                {
                    PlayerInfo = x.ToSimpleProto(FriendOnlineStatus.Online)
                })
            }
        };

        SetData(proto);
    }
}