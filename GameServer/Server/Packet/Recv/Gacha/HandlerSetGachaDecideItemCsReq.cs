using EggLink.DanhengServer.GameServer.Server.Packet.Send.Gacha;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Gacha;

[Opcode(CmdIds.SetGachaDecideItemCsReq)]
public class HandlerSetGachaDecideItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetGachaDecideItemCsReq.Parser.ParseFrom(data);

        // DPKOPFDEFMN 原名为 DecideItemOrder
        connection.Player!.GachaManager!.GachaData.GachaDecideOrder = req.DPKOPFDEFMN.Select(x => (int)x).ToList();

        await connection.SendPacket(new PacketSetGachaDecideItemScRsp(req.GachaId, req.DPKOPFDEFMN.ToList()));
    }
}