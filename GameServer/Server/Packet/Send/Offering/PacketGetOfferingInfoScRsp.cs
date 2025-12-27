using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Offering;

public class PacketGetOfferingInfoScRsp : BasePacket
{
    public PacketGetOfferingInfoScRsp(List<OfferingTypeData> dataList) : base(CmdIds.GetOfferingInfoScRsp)
    {
        var proto = new GetOfferingInfoScRsp
        {
            OfferingInfoList = { dataList.Select(data => data.ToProto()).ToList() }
        };

        SetData(proto);
    }
}