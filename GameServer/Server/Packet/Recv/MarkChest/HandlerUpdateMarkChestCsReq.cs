using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.MarkChest;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.MarkChest;

[Opcode(CmdIds.UpdateMarkChestCsReq)]
public class HandlerUpdateMarkChestCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdateMarkChestCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;

        List<SceneMarkedChestData> markedChestData = [];

        foreach (var markChestInfo in req.MarkChestInfoList)
            markedChestData.Add(new SceneMarkedChestData
            {
                ConfigId = (int)markChestInfo.ConfigId,
                FloorId = (int)markChestInfo.FloorId,
                GroupId = (int)markChestInfo.GroupId,
                PlaneId = (int)markChestInfo.PlaneId
            });

        foreach (var chestData in (player.SceneData!.MarkedChestData.GetValueOrDefault((int)req.FuncId) ?? []).Where(
                     chestData => markedChestData.All(x =>
                         !(x.ConfigId == chestData.ConfigId && x.FloorId == chestData.FloorId &&
                           x.GroupId == chestData.GroupId))))
            // Add the existing marked chest data if it is not in the new marked chest data
            markedChestData.Add(chestData);

        player.SceneData!.MarkedChestData[(int)req.FuncId] = markedChestData;

        await connection.SendPacket(new PacketUpdateMarkChestScRsp(req.FuncId, player));
    }
}