using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Offering;

public class PacketSubmitOfferingItemScRsp : BasePacket
{
    public PacketSubmitOfferingItemScRsp(Retcode ret, OfferingTypeData? data) : base(CmdIds.SubmitOfferingItemScRsp)
    {
        var proto = new SubmitOfferingItemScRsp
        {
            Retcode = (uint)ret
        };

        if (data != null) proto.OfferingInfo = data.ToProto();

        SetData(proto);
    }
}