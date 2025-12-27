using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightGeneralScRsp : BasePacket
{
    public PacketFightGeneralScRsp(uint networkType) : base(CmdIds.FightGeneralScRsp)
    {
        var proto = new FightGeneralScRsp
        {
            NAFFAFEHPFK = networkType
        };

        SetData(proto);
    }

    public PacketFightGeneralScRsp(Retcode code) : base(CmdIds.FightGeneralScRsp)
    {
        var proto = new FightGeneralScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}