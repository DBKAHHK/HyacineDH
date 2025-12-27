using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketDeleteFriendScRsp : BasePacket
{
    public PacketDeleteFriendScRsp() : base(CmdIds.DeleteFriendScRsp)
    {
        var proto = new DeleteFriendScRsp();

        SetData(proto);
    }

    public PacketDeleteFriendScRsp(uint uid) : base(CmdIds.DeleteFriendScRsp)
    {
        var proto = new DeleteFriendScRsp
        {
            Uid = uid
        };

        SetData(proto);
    }
}