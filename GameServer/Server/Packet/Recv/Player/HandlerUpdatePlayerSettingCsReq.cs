using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.UpdatePlayerSettingCsReq)]
public class HandlerUpdatePlayerSettingCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdatePlayerSettingCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketUpdatePlayerSettingScRsp(req.PlayerSetting));
    }
}