using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketResetEraFlipperDataScRsp : BasePacket
{
    public PacketResetEraFlipperDataScRsp(int regionId, int state, bool leave) : base(CmdIds.ResetEraFlipperDataScRsp)
    {
        var proto = new ResetEraFlipperDataScRsp
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
            CEICIHHHJDC = leave
        };

        SetData(proto);
    }

    public PacketResetEraFlipperDataScRsp(Retcode code) : base(CmdIds.ResetEraFlipperDataScRsp)
    {
        var proto = new ResetEraFlipperDataScRsp
        {
            Retcode = (uint)code
        };
        SetData(proto);
    }
}