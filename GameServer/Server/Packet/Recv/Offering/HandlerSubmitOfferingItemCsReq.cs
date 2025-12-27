using EggLink.DanhengServer.GameServer.Server.Packet.Send.Offering;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Offering;

[Opcode(CmdIds.SubmitOfferingItemCsReq)]
public class HandlerSubmitOfferingItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SubmitOfferingItemCsReq.Parser.ParseFrom(data);

        var res = await connection.Player!.OfferingManager!.SubmitOfferingItem((int)req.OfferingId);

        await connection.SendPacket(new PacketSubmitOfferingItemScRsp(res.Item1, res.data));
    }
}