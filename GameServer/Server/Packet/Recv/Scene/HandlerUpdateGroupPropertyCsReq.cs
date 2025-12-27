using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.UpdateGroupPropertyCsReq)]
public class HandlerUpdateGroupPropertyCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdateGroupPropertyCsReq.Parser.ParseFrom(data);

        if (req.FloorId != connection.Player!.SceneInstance!.FloorId)
        {
            await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(Retcode.RetReqParaInvalid));
            return;
        }

        // try to get group
        var scene = connection.Player.SceneInstance;
        if (!scene.Groups.Contains((int)req.GroupId))
        {
            await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(Retcode.RetGroupNotExist));
            return;
        }

        // update group property
        var res = await scene.UpdateGroupProperty((int)req.GroupId, req.DELEKFMGGCM, req.HGJDNJEACBE);
        await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(res, req));
    }
}