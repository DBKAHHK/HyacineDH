using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketGetBigDataAllRecommendScRsp : BasePacket
{
    public PacketGetBigDataAllRecommendScRsp(BigDataRecommendType type) : base(CmdIds.GetBigDataAllRecommendScRsp)
    {
        var proto = new GetBigDataAllRecommendScRsp
        {
            BigDataRecommendType = type
        };

        SetData(proto);
    }
}