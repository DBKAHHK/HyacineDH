using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.SetFriendMarkCsReq)]
public class HandlerSetFriendMarkCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetFriendMarkCsReq.Parser.ParseFrom(data);

        // IINLAPPNAGB 原名为 ADJGKCOKOLN
        connection.Player!.FriendManager!.MarkFriend((int)req.Uid, req.IINLAPPNAGB);

        await connection.SendPacket(new PacketSetFriendMarkScRsp(req.Uid, req.IINLAPPNAGB));
    }
}