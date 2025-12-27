using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetFriendLoginInfoCsReq)]
public class HandlerGetFriendLoginInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var friends = connection.Player!.FriendManager!
            .GetFriendPlayerData().Select(x => x.Uid).ToList();

        await connection.SendPacket(new PacketGetFriendLoginInfoScRsp(friends));
    }
}