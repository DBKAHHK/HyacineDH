using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;

public class PacketRelicSmartWearGetPinRelicScRsp : BasePacket
{
    public PacketRelicSmartWearGetPinRelicScRsp(uint avatarId) : base(CmdIds.RelicSmartWearGetPinRelicScRsp)
    {
        var proto = new RelicSmartWearGetPinRelicScRsp
        {
            AvatarId = avatarId
        };

        SetData(proto);
    }
}