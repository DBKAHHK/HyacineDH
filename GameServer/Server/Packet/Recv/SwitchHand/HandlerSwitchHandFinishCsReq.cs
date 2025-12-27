using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandFinishCsReq)]
public class HandlerSwitchHandFinishCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var component = connection.Player!.GetComponent<SwitchHandComponent>();

        var info = component.GetHandInfo(component.RunningHandConfigId);
        component.RunningHandConfigId = 0;
        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandFinishScRsp(info.Item1));
        }
        else
        {
            await connection.SendPacket(new PacketSwitchHandFinishScRsp(info.Item2));
        }
    }
}