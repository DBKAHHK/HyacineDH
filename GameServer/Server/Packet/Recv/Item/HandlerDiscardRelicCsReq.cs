using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.DiscardRelicCsReq)]
public class HandlerDiscardRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DiscardRelicCsReq.Parser.ParseFrom(data);
        var result =
            // RelicIds 原名为 RelicUniqueIdList
            // LMCLFMGALJB 原名为 IsDiscard
            await connection.Player!.InventoryManager!.DiscardItems(req.RelicIds, req.LMCLFMGALJB,
                ItemMainTypeEnum.Relic);
        await connection.SendPacket(new PacketDiscardRelicScRsp(result, req.LMCLFMGALJB));
    }      
}