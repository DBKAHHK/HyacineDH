using EggLink.DanhengServer.GameServer.Game.Scene.Component;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipper;

[Opcode(CmdIds.ResetEraFlipperDataCsReq)]
public class HandlerResetEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ResetEraFlipperDataCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.SceneInstance!.GetComponent<EraFlipperSceneComponent>();
        if (component == null)
        {
            await connection.SendPacket(new PacketResetEraFlipperDataScRsp(Retcode.RetAdventureMapNotExist));
            return;
        }

        // leave
        // CEICIHHHJDC 原名为 PAHMAGPFDDJ
        await connection.SendPacket(new PacketResetEraFlipperDataScRsp(component.CurRegionId, component.RegionState, req.CEICIHHHJDC));

        component.LeaveFlipperRegion();
    }
}