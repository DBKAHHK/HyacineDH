using EggLink.DanhengServer.Kcp;

using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.PlayerLoginFinishCsReq)]
public class HandlerPlayerLoginFinishCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.PlayerLoginFinishScRsp);
        await connection.SendPacket(new PacketClientDownloadDataScNotify());
        //var list = connection.Player!.MissionManager!.GetRunningSubMissionIdList();
        //await connection.SendPacket(new PacketMonthCardRewardNotify([new ItemData
        //{
        //    ItemId = 1,
        //    Count = 90
        //}]));
    }
}
