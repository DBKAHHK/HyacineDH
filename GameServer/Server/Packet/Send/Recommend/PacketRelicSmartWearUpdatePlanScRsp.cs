using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketRelicSmartWearUpdatePlanScRsp : BasePacket
{
    public PacketRelicSmartWearUpdatePlanScRsp(RelicSmartWearPlan relicPlan)
        : base(CmdIds.RelicSmartWearUpdatePlanScRsp)
    {
        var proto = new RelicSmartWearUpdatePlanScRsp
        {
            RelicPlan = relicPlan
        };

        SetData(proto);
    }
}