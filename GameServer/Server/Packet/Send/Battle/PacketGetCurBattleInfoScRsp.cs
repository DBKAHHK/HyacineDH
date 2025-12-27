using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Battle;

public class PacketGetCurBattleInfoScRsp : BasePacket
{
    public PacketGetCurBattleInfoScRsp(EggLink.DanhengServer.GameServer.Game.Player.PlayerInstance? player) : base(CmdIds.GetCurBattleInfoScRsp)
    {
        var battle = player?.BattleInstance;

        var proto = new GetCurBattleInfoScRsp();
        if (battle != null)
        {
            proto.BattleInfo = battle.ToProto();
            proto.Retcode = 0;
        }
        else
        {
            proto.BattleInfo = new SceneBattleInfo();
            proto.Retcode = 1;
        }

        SetData(proto);
    }
}
