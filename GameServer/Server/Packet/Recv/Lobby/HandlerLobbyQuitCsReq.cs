using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyQuitCsReq)]
public class HandlerLobbyQuitCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var room = ServerUtils.LobbyServerManager.GetPlayerJoinedRoom(connection.Player!.Uid);
        if (room == null)
        {
            await connection.SendPacket(new PacketLobbyQuitScRsp(Retcode.RetLobbyRoomNotExist));
            return;
        }

        await room.RemovePlayer(connection.Player.Uid);
        await connection.SendPacket(new PacketLobbyQuitScRsp(Retcode.RetSucc));
    }
}