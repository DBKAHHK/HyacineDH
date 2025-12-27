using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketUnlockAvatarSkinScNotify : BasePacket
{
    public PacketUnlockAvatarSkinScNotify(int skinId) : base(CmdIds.UnlockAvatarSkinScNotify)
    {
        var proto = new UnlockAvatarSkinScNotify
        {
            SkinId = (uint)skinId
        };

        SetData(proto);
    }
}