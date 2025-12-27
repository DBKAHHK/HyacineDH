using EggLink.DanhengServer.GameServer.Game.RogueTourn;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueTourn;

public class PacketRogueTournTitanUpdateTitanBlessProgressScNotify : BasePacket
{
    public PacketRogueTournTitanUpdateTitanBlessProgressScNotify(RogueTournInstance inst) : base(
        CmdIds.RogueTournTitanUpdateTitanBlessProgressScNotify)
    {
        var proto = new RogueTournTitanUpdateTitanBlessProgressScNotify
        {
            TitanBlessProgress = (uint)inst.TitanProgress
        };

        SetData(proto);
    }
}