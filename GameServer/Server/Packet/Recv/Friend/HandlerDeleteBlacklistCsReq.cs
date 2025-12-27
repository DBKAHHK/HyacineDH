using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.DeleteBlacklistCsReq)]
public class HandlerDeleteBlacklistCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DeleteBlacklistCsReq.Parser.ParseFrom(data);

        connection.Player!.FriendManager!.RemoveBlackList((int)req.Uid);

        await connection.SendPacket(new PacketDeleteBlacklistScRsp(req.Uid));
    }
}