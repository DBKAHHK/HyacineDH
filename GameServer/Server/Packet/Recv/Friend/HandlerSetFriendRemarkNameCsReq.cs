using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.SetFriendRemarkNameCsReq)]
public class HandlerSetFriendRemarkNameCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetFriendRemarkNameCsReq.Parser.ParseFrom(data);

        connection.Player!.FriendManager!.RemarkFriendName((int)req.Uid, req.RemarkName);

        await connection.SendPacket(new PacketSetFriendRemarkNameScRsp(req.Uid, req.RemarkName));
    }
}