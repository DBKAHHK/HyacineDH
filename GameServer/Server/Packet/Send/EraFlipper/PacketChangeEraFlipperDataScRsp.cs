using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketChangeEraFlipperDataScRsp : BasePacket
{
    public PacketChangeEraFlipperDataScRsp(ChangeEraFlipperDataCsReq req) : base(CmdIds.ChangeEraFlipperDataScRsp)
    {
        var proto = new ChangeEraFlipperDataScRsp
        {
            Data = req.Data
        };

        SetData(proto);
    }

    public PacketChangeEraFlipperDataScRsp(Retcode code) : base(CmdIds.ChangeEraFlipperDataScRsp)
    {
        var proto = new ChangeEraFlipperDataScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}