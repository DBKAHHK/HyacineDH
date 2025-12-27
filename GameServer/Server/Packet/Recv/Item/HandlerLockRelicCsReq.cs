using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.LockRelicCsReq)]
public class HandlerLockRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LockRelicCsReq.Parser.ParseFrom(data);
        // RelicIds 原名为 RelicUniqueIdList
        // HIIBGFLBKCI 原名为 IsProtected
        var result =
            await connection.Player!.InventoryManager!.LockItems(req.RelicIds, req.HIIBGFLBKCI,
                ItemMainTypeEnum.Relic);
        await connection.SendPacket(new PacketLockRelicScRsp(result));
    }
}