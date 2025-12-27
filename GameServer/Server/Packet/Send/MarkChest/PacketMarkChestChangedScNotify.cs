using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.MarkChest;

public class PacketMarkChestChangedScNotify : BasePacket
{
    public PacketMarkChestChangedScNotify(PlayerInstance player) : base(CmdIds.MarkChestChangedScNotify)
    {
        var proto = new MarkChestChangedScNotify
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