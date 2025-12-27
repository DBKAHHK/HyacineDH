using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketSyncDeleteFriendScNotify : BasePacket
{
    public PacketSyncDeleteFriendScNotify(int uid)
        : base(CmdIds.SyncDeleteFriendScNotify)
    {
        var proto = new SyncDeleteFriendScNotify
        {
            Uid = (uint)uid
        };

        SetData(proto);
    }
}