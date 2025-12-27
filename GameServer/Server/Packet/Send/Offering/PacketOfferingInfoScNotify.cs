using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Offering;

public class PacketOfferingInfoScNotify : BasePacket
{
    public PacketOfferingInfoScNotify(OfferingTypeData data) : base(CmdIds.OfferingInfoScNotify)
    {
        var proto = new OfferingInfoScNotify
        {
            OfferingInfo = data.ToProto()
        };

        SetData(proto);
    }
}