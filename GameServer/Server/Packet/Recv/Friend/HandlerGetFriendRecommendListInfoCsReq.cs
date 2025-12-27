using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetFriendRecommendListInfoCsReq)]
public class HandlerGetFriendRecommendListInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var friends = connection.Player!.FriendManager!.GetRandomFriend();

        await connection.SendPacket(new PacketGetFriendRecommendListInfoScRsp(friends));
    }
}