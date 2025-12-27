using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.LockEquipmentCsReq)]
public class HandlerLockEquipmentCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LockEquipmentCsReq.Parser.ParseFrom(data);
        // IsLocked 原名为 IsProtected
        var result =
            await connection.Player!.InventoryManager!.LockItems(req.EquipmentIdList, req.IsLocked,
                ItemMainTypeEnum.Equipment);
        await connection.SendPacket(new PacketLockEquipmentScRsp(result));
    }
}