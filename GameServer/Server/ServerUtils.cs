using EggLink.DanhengServer.GameServer.Game.Lobby;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer;

namespace EggLink.DanhengServer.GameServer.Server;

public static class ServerUtils
{
    public static LobbyServerManager LobbyServerManager { get; set; } = new();
    public static MultiPlayerGameServerManager MultiPlayerGameServerManager { get; set; } = new();
}