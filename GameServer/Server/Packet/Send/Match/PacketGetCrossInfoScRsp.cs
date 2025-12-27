using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;

public class PacketGetCrossInfoScRsp : BasePacket
{
    public PacketGetCrossInfoScRsp() : base(CmdIds.GetCrossInfoScRsp)
    {
        var proto = new GetCrossInfoScRsp
        {
            // GCLCCOGIDOA 原名为 FightGameMode
            // GCLCCOGIDOA 原名为 FightGameMode
            GCLCCOGIDOA = FightGameMode.Marble
        };

        SetData(proto);
    }
}