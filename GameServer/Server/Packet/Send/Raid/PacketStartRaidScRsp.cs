using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Raid;

public class PacketStartRaidScRsp : BasePacket
{
    public PacketStartRaidScRsp(RaidRecord record, PlayerInstance player) : base(CmdIds.StartRaidScRsp)
    {
        var proto = new StartRaidScRsp
        {
            // GBHIHPKNBCA 原名为 RaidPlayerData
            Scene = new GBHIHPKNBCA
            {
                Lineup = player.LineupManager!.GetCurLineup()!.ToProto(),
                RaidId = (uint)record.RaidId,
                // ADEJKMNENAN 原名为 RaidSceneInfo
                ADEJKMNENAN = player.SceneInstance!.ToProto(),
                WorldLevel = (uint)record.WorldLevel
            }
        };

        SetData(proto);
    }

    public PacketStartRaidScRsp(Retcode ret) : base(CmdIds.StartRaidScRsp)
    {
        var proto = new StartRaidScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}