using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyModifyPlayerInfoCsReq)]
public class HandlerLobbyModifyPlayerInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LobbyModifyPlayerInfoCsReq.Parser.ParseFrom(data);

        var room = ServerUtils.LobbyServerManager.GetPlayerJoinedRoom(connection.Player!.Uid);
        if (room == null)
        {
            await connection.SendPacket(new PacketLobbyModifyPlayerInfoScRsp(Retcode.RetLobbyRoomNotExist));
            return;
        }

        var player = room.GetPlayerByUid(connection.Player.Uid);
        if (player == null)
        {
            await connection.SendPacket(new PacketLobbyModifyPlayerInfoScRsp(Retcode.RetLobbyRoomNotExist));
            return;
        }

        player.EquippedSealList = req.LobbyGameInfo.LobbyMarbleInfo.LobbySealList.Select(x => (int)x).ToList();
        player.CharacterStatus = req.Type switch
        {
            LobbyModifyType.Ready => LobbyCharacterStatus.Ready,
            LobbyModifyType.Operating => LobbyCharacterStatus.Operating,
            _ => player.CharacterStatus
        };

        await room.BroadCastToRoom(new PacketLobbySyncInfoScNotify(player.Player.Uid, room, req.Type));
        await connection.SendPacket(new PacketLobbyModifyPlayerInfoScRsp(Retcode.RetSucc));
    }
}