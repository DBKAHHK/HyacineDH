using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Multiplayer;

[Opcode(CmdIds.MultiplayerFightGiveUpCsReq)]
public class HandlerMultiplayerFightGiveUpCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = MultiplayerFightGiveUpCsReq.Parser.ParseFrom(data);

        var roomId = req.GateRoomId;
        ServerUtils.MultiPlayerGameServerManager.Rooms.TryGetValue((long)roomId, out var room);
        var player = room?.GetPlayerById(connection.MarblePlayer?.LobbyPlayer.Player.Uid ?? connection.Player!.Uid);
        if (player != null)
            player.LeaveGame = true;

        await connection.SendPacket(CmdIds.MultiplayerFightGiveUpScRsp);
    }
}