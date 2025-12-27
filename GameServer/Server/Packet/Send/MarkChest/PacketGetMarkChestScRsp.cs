using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.MarkChest;

public class PacketGetMarkChestScRsp : BasePacket
{
    public PacketGetMarkChestScRsp(PlayerInstance player) : base(CmdIds.GetMarkChestScRsp)
    {
        var proto = new GetMarkChestScRsp
        {
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