using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;

public class PacketLockEquipmentScRsp : BasePacket
{
    public PacketLockEquipmentScRsp(bool success) : base(CmdIds.LockEquipmentScRsp)
    {
        LockEquipmentScRsp proto = new();

        if (!success) proto.Retcode = (uint)Retcode.RetFail;

        SetData(proto);
    }
}