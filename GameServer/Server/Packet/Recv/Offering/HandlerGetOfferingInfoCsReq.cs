using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Offering;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Offering;

[Opcode(CmdIds.GetOfferingInfoCsReq)]
public class HandlerGetOfferingInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetOfferingInfoCsReq.Parser.ParseFrom(data);

        List<OfferingTypeData> dataList = [];
        dataList.AddRange(req.OfferingIdList.Select(id => connection.Player!.OfferingManager!.GetOfferingData((int)id))
            .OfType<OfferingTypeData>());

        await connection.SendPacket(new PacketGetOfferingInfoScRsp(dataList));
    }
}