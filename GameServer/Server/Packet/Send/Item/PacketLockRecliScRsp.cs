using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;

public class PacketLockRelicScRsp : BasePacket
{
    public PacketLockRelicScRsp(bool success) : base(CmdIds.LockRelicScRsp)
    {
        LockRelicScRsp proto = new();

        if (!success) proto.Retcode = (uint)Retcode.RetFail;

        SetData(proto);
    }
}