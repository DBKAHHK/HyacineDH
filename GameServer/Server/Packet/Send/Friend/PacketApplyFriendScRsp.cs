using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketApplyFriendScRsp : BasePacket
{
    public PacketApplyFriendScRsp(Retcode ret, uint uid) : base(CmdIds.ApplyFriendScRsp)
    {
        var proto = new ApplyFriendScRsp
        {
            Retcode = (uint)ret,
            Uid = uid
        };

        SetData(proto);
    }
}