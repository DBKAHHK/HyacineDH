using EggLink.DanhengServer.GameServer.Server.Packet.Send.Phone;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Phone;

[Opcode(CmdIds.SetPersonalCardCsReq)]
public class HandlerSetPersonalCardCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetPersonalCardCsReq.Parser.ParseFrom(data);

        connection.Player!.Data.PersonalCard = (int)req.Id;

        await connection.SendPacket(new PacketSetPersonalCardScRsp(req.Id));
    }
}