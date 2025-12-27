using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyJoinCsReq)]
public class HandlerLobbyJoinCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LobbyJoinCsReq.Parser.ParseFrom(data);

        var room = ServerUtils.LobbyServerManager.LobbyRoomInstances.GetValueOrDefault((long)req.RoomId);

        if (room == null)
        {
            await connection.SendPacket(new PacketLobbyJoinScRsp(Retcode.RetLobbyRoomNotExist));
            return;
        }

        if (room.Players.Count >= 2)
        {
            await connection.SendPacket(new PacketLobbyJoinScRsp(Retcode.RetLobbyRoomPalyerFull));
            return;
        }

        var player = room.GetPlayerByUid(connection.Player!.Uid);
        if (player != null)
        {
            await connection.SendPacket(new PacketLobbyJoinScRsp(Retcode.RetLobbyRoomPalyerFull));
            return;
        }

        await room.AddPlayer(connection.Player!,
            req.LobbyGameInfo.LobbyMarbleInfo.LobbySealList.Select(x => (int)x).ToList(),
            LobbyCharacterType.LobbyCharacterMember);

        await connection.SendPacket(new PacketLobbyJoinScRsp(room));
    }
}