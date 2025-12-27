using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketReserveStaminaExchangeScRsp : BasePacket
{
    public PacketReserveStaminaExchangeScRsp(uint amount) : base(CmdIds.ReserveStaminaExchangeScRsp)
    {
        var proto = new ReserveStaminaExchangeScRsp();

        if (amount > 0) proto.Num = amount;
        else proto.Retcode = (uint)Retcode.RetFail;

        SetData(proto);
    }
}