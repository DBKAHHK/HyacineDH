using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Fight;

[Opcode(CmdIds.FightHeartBeatCsReq)]
public class HandlerFightHeartBeatCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FightHeartBeatCsReq.Parser.ParseFrom(data);

        if (connection.MarbleRoom != null) await connection.MarbleRoom.OnPlayerHeartBeat();

        await connection.SendPacket(new PacketFightHeartBeatScRsp(req.ClientTimeMs));
    }
}