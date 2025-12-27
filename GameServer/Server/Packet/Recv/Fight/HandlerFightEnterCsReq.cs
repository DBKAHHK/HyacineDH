using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using EggLink.DanhengServer.Util.Security;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Fight;

[Opcode(CmdIds.FightEnterCsReq)]
public class HandlerFightEnterCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FightEnterCsReq.Parser.ParseFrom(data);
        // JKFEKLKBGJG 原名为 EnterRoomId
        if (!ServerUtils.MultiPlayerGameServerManager.Rooms.TryGetValue((long)req.JKFEKLKBGJG, out var room))
        {
            await connection.SendPacket(new PacketFightEnterScRsp(Retcode.RetFightRoomNotExist));
            connection.Stop();
            return;
        }

        if (room is not MarbleGameRoomInstance marbleGame)
        {
            await connection.SendPacket(new PacketFightEnterScRsp(Retcode.RetFightRoomNotExist));
            connection.Stop();
            return;
        }

        var player = room.GetPlayerById((int)req.Uid);
        if (player is not MarbleGamePlayerInstance marble)
        {
            await connection.SendPacket(new PacketFightEnterScRsp(Retcode.RetFightRoomNotExist));
            connection.Stop();
            return;
        }

        connection.MarblePlayer = marble;
        marble.Connection = connection;
        connection.MarbleRoom = marbleGame;

        if (ConfigManager.Config.GameServer.UsePacketEncryption)
        {
            var tempRandom = new MT19937((ulong)DateTimeOffset.Now.ToUnixTimeSeconds());
            connection.ClientSecretKeySeed = tempRandom.NextUInt64();
        }

        connection.State = SessionStateEnum.ACTIVE;
        connection.DebugFile = Path.Combine(ConfigManager.Config.Path.LogPath, "Debug/", $"{req.Uid}/",
            $"Debug-{DateTime.Now:yyyy-MM-dd HH-mm-ss}-FightGame.log");

        await connection.SendPacket(new PacketFightEnterScRsp(connection.ClientSecretKeySeed));

        if (ConfigManager.Config.GameServer.UsePacketEncryption)
            connection.XorKey = Crypto.GenerateXorKey(connection.ClientSecretKeySeed);

        await marbleGame.EnterGame(player.LobbyPlayer.Player.Uid);
    }
}