using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.MarkChest;

public class PacketUpdateMarkChestScRsp : BasePacket
{
    public PacketUpdateMarkChestScRsp(uint funcId, PlayerInstance player) : base(CmdIds.UpdateMarkChestScRsp)
    {
        var proto = new UpdateMarkChestScRsp
        {
            FuncId = funcId,
            MarkChestFuncInfo =
            {
                player.SceneData!.MarkedChestData.Select(x => new MarkChestFuncInfo
                {
                    FuncId = (uint)x.Key,
                    MarkChestInfoList = { x.Value.Select(y => y.ToProto()) }
                })
            }
        };

        SetData(proto);
    }
}