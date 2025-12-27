using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyCreateCsReq)]
public class HandlerLobbyCreateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LobbyCreateCsReq.Parser.ParseFrom(data);

        if (req.FightGameMode != FightGameMode.Marble)
        {
            await connection.SendPacket(new PacketLobbyCreateScRsp(Retcode.RetTimeout));
            return;
        }

        var lobbyMode = req.LobbyMode;
        var marbleList = req.LobbyGameInfo.LobbyMarbleInfo.LobbySealList.Select(x => (int)x).ToList();

        var room = await ServerUtils.LobbyServerManager.CreateLobbyRoom(connection.Player!, (int)lobbyMode, marbleList);
        await connection.SendPacket(new PacketLobbyCreateScRsp(room));
    }
}