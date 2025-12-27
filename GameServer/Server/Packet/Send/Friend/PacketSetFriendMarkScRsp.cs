using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;

public class PacketSetFriendMarkScRsp : BasePacket
{
    public PacketSetFriendMarkScRsp(uint uid, bool isMark) : base(CmdIds.SetFriendMarkScRsp)
    {
        var proto = new SetFriendMarkScRsp
        {
            Uid = uid,
            IINLAPPNAGB = isMark
        };

        SetData(proto);
    }
}