using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketEnterEraFlipperRegionScRsp : BasePacket
{
    public PacketEnterEraFlipperRegionScRsp(uint regionId) : base(CmdIds.EnterEraFlipperRegionScRsp)
    {
        var proto = new EnterEraFlipperRegionScRsp
        {
            EraFlipperRegionId = regionId
        };

        SetData(proto);
    }

    public PacketEnterEraFlipperRegionScRsp(Retcode code) : base(CmdIds.EnterEraFlipperRegionScRsp)
    {
        var proto = new EnterEraFlipperRegionScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}