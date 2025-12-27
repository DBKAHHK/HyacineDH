using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketDeleteBlacklistScRsp : BasePacket
{
    public PacketDeleteBlacklistScRsp(uint uid) : base(CmdIds.DeleteBlacklistScRsp)
    {
        var proto = new DeleteBlacklistScRsp
        {
            Uid = uid
        };

        SetData(proto);
    }
}