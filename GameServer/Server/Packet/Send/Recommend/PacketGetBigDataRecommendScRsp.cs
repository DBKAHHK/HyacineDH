using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketGetBigDataRecommendScRsp : BasePacket
{
    public PacketGetBigDataRecommendScRsp(uint avatarId, BigDataRecommendType type)
        : base(CmdIds.GetBigDataRecommendScRsp)
    {
        var proto = new GetBigDataRecommendScRsp
        {
            HasRecommand = true,
            EquipAvatar = avatarId,
            BigDataRecommendType = type
        };

        SetData(proto);
    }
}