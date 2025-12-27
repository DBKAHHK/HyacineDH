using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketRelicSmartWearDeletePlanScRsp : BasePacket
{
    public PacketRelicSmartWearDeletePlanScRsp(uint uniqueId)
        : base(CmdIds.RelicSmartWearDeletePlanScRsp)
    {
        var proto = new RelicSmartWearDeletePlanScRsp
        {
            UniqueId = uniqueId
        };

        SetData(proto);
    }
}