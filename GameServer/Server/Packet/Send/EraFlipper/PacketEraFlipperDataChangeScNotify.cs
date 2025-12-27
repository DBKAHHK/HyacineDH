using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketEraFlipperDataChangeScNotify : BasePacket
{
    public PacketEraFlipperDataChangeScNotify(ChangeEraFlipperDataCsReq req, int floorId) : base(
        CmdIds.EraFlipperDataChangeScNotify)
    {
        var proto = new EraFlipperDataChangeScNotify
        {
            Data = req.Data,
            FloorId = (uint)floorId
        };

        SetData(proto);
    }

    public PacketEraFlipperDataChangeScNotify(int floorId, int regionId, int state) : base(
        CmdIds.EraFlipperDataChangeScNotify)
    {
        var proto = new EraFlipperDataChangeScNotify
        {
            Data = new EraFlipperDataList
            {
                // DMFMNPMEHKM 原名为 EraFlipperData 列表
                DMFMNPMEHKM =
                {
                    // IPKNDKKGOGF 原名为 EraFlipperData
                    new IPKNDKKGOGF
                    {
                        // EraFlipperRegionId 保持原名
                        EraFlipperRegionId = (uint)regionId,
                        // State 保持原名
                        State = (uint)state
                    }
                }
            },
            FloorId = (uint)floorId
        };

        SetData(proto);
    }

    public PacketEraFlipperDataChangeScNotify(int floorId) : base(CmdIds.EraFlipperDataChangeScNotify)
    {
        var proto = new EraFlipperDataChangeScNotify
        {
            FloorId = (uint)floorId
        };

        SetData(proto);
    }
}