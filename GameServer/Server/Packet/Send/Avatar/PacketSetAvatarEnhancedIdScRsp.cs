using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketSetAvatarEnhancedIdScRsp : BasePacket
{
    public PacketSetAvatarEnhancedIdScRsp(Retcode retcode) : base(CmdIds.SetAvatarEnhancedIdScRsp)
    {
        var proto = new SetAvatarEnhancedIdScRsp
        {
            Retcode = (uint)retcode
        };

        SetData(proto);
    }

    public PacketSetAvatarEnhancedIdScRsp(uint avatarId, int enhanceId) : base(CmdIds.SetAvatarEnhancedIdScRsp)
    {
        var proto = new SetAvatarEnhancedIdScRsp
        {
            UnkEnhancedId = (uint)enhanceId,
            GrowthAvatarId = avatarId
        };

        SetData(proto);
    }
}