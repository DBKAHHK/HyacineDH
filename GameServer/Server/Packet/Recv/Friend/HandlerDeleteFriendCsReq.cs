using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.DeleteFriendCsReq)]
public class HandlerDeleteFriendCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DeleteFriendCsReq.Parser.ParseFrom(data);

        var uid = await connection.Player!.FriendManager!.RemoveFriend((int)req.Uid);
        if (uid == null)
            await connection.SendPacket(new PacketDeleteFriendScRsp());
        else
            await connection.SendPacket(new PacketDeleteFriendScRsp((uint)uid));
    }
}