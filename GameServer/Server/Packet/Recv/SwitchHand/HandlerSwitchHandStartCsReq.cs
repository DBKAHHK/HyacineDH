using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandStartCsReq)]
public class HandlerSwitchHandStartCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandStartCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();
        component.RunningHandConfigId = (int)req.ConfigId;

        await connection.SendPacket(new PacketSwitchHandStartScRsp(req.ConfigId));
    }
}