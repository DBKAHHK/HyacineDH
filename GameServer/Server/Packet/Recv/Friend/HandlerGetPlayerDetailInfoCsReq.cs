using EggLink.DanhengServer.GameServer.Server.Packet.Send.Friend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetPlayerDetailInfoCsReq)]
public class HandlerGetPlayerDetailInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetPlayerDetailInfoCsReq.Parser.ParseFrom(data);

        var playerData = connection.Player!.FriendManager!.GetFriendPlayerData([(int)req.Uid]).First();
        if (playerData == null)
        {
            await connection.SendPacket(new PacketGetPlayerDetailInfoScRsp());
            return;
        }

        await connection.SendPacket(new PacketGetPlayerDetailInfoScRsp(playerData.ToDetailProto()));
    }
}