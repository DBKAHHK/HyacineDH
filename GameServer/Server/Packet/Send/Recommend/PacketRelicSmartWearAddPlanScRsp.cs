using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketRelicSmartWearAddPlanScRsp : BasePacket
{
    public PacketRelicSmartWearAddPlanScRsp(RelicSmartWearPlan addPlan) : base(CmdIds.RelicSmartWearAddPlanScRsp)
    {
        var proto = new RelicSmartWearAddPlanScRsp
        {
            RelicPlan = addPlan
        };

        SetData(proto);
    }
}