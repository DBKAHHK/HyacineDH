using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.StartCocoonStageCsReq)]
public class HandlerStartCocoonStageCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = StartCocoonStageCsReq.Parser.ParseFrom(data);
        var battle =
            await connection.Player!.BattleManager!.StartCocoonStage((int)req.CocoonId, (int)req.Wave,
                (int)req.WorldLevel);
        connection.Player.SceneInstance?.OnEnterStage();

        if (battle != null)
            await connection.SendPacket(new PacketStartCocoonStageScRsp(battle, (int)req.CocoonId, (int)req.Wave));
        else
            await connection.SendPacket(new PacketStartCocoonStageScRsp());
    }
}