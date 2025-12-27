using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketSyncAddBlacklistScNotify : BasePacket
{
    public PacketSyncAddBlacklistScNotify(int uid)
        : base(CmdIds.SyncAddBlacklistScNotify)
    {
        var proto = new SyncAddBlacklistScNotify
        {
            Uid = (uint)uid
        };

        SetData(proto);
    }
}