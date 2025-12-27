using EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Multiplayer;

[Opcode(CmdIds.MultiplayerGetFightGateCsReq)]
public class HandlerMultiplayerGetFightGateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var room = ServerUtils.MultiPlayerGameServerManager.GetPlayerJoinedRoom(connection.Player!.Uid);
        if (room == null)
        {
            await connection.SendPacket(new PacketMultiplayerGetFightGateScRsp(Retcode.RetFightRoomNotExist));
            return;
        }

        await connection.SendPacket(new PacketMultiplayerGetFightGateScRsp(room));
    }
}