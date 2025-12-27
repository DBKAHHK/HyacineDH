using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketOpenChestScNotify : BasePacket
{
    public PacketOpenChestScNotify(int chestId) : base(CmdIds.OpenChestScNotify)
    {
        var proto = new OpenChestScNotify
        {
            ChestId = (uint)chestId
        };

        SetData(proto);
    }
}