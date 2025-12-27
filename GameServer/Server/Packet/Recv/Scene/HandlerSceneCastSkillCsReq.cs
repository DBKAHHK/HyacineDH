using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.SceneCastSkillCsReq)]
public class HandlerSceneCastSkillCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SceneCastSkillCsReq.Parser.ParseFrom(data);

        var player = connection.Player!;
        var res = await player.SceneSkillManager!.OnCast(req);

        await connection.SendPacket(new PacketSceneCastSkillScRsp(res.RetCode, req.CastEntityId, res.Instance,
            res.TriggerBattleInfos ?? []));
        if (res.Instance != null) await player.SceneInstance!.OnEnterStage();
    }
}