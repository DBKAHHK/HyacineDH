using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Adventure;

public class PacketQuickStartCocoonStageScRsp : BasePacket
{
    public PacketQuickStartCocoonStageScRsp() : base(CmdIds.QuickStartCocoonStageScRsp)
    {
        var rsp = new QuickStartCocoonStageScRsp
        {
            Retcode = 1
        };

        SetData(rsp);
    }

    public PacketQuickStartCocoonStageScRsp(BattleInstance battle, int cocoonId, int wave) : base(
        CmdIds.QuickStartCocoonStageScRsp)
    {
        var rsp = new QuickStartCocoonStageScRsp
        {
            CocoonId = (uint)cocoonId,
            Wave = (uint)wave,
            BattleInfo = battle.ToProto()
        };

        SetData(rsp);
    }
}