using EggLink.DanhengServer.GameServer.Game.Lobby;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer;

public class MultiPlayerGameServerManager
{
    public Dictionary<long, BaseMultiPlayerGameRoomInstance> Rooms { get; } = [];
    public long CurRoomId { get; set; }

    public BaseMultiPlayerGameRoomInstance? CreateRoom(LobbyRoomInstance lobbyRoom)
    {
        if (lobbyRoom.GameMode != FightGameMode.Marble) return null;
        var roomId = ++CurRoomId;
        var room = new MarbleGameRoomInstance(roomId, lobbyRoom);
        Rooms.Add(roomId, room);

        return room;
    }

    public void RemoveRoom(long roomId)
    {
        Rooms.Remove(roomId, out _);
    }

    public BaseMultiPlayerGameRoomInstance? GetPlayerJoinedRoom(int uid)
    {
        foreach (var room in Rooms.Values)
            if (room.Players.Any(x => !x.LeaveGame && x.LobbyPlayer.Player.Uid == uid))
                return room;

        return null;
    }
}