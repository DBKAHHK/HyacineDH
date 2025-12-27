using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketGetEraFlipperDataScRsp : BasePacket
{
    public PacketGetEraFlipperDataScRsp(PlayerInstance player) : base(CmdIds.GetEraFlipperDataScRsp)
    {
        var proto = new GetEraFlipperDataScRsp
        {
            Data = new EraFlipperDataList
            {
                // DMFMNPMEHKM 原名为 EraFlipperData 列表
                DMFMNPMEHKM =
                {
                    // IPKNDKKGOGF 原名为 EraFlipperData
                    player.SceneData!.EraFlipperData.RegionState.Select(x => new IPKNDKKGOGF
                    {
                        // EraFlipperRegionId 保持原名
                        EraFlipperRegionId = (uint)x.Key,
                        // State 保持原名
                        State = (uint)x.Value
                    })
                }
            }
        };

        SetData(proto);
    }
}