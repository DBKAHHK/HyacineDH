using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandResetGameCsReq)]
public class HandlerSwitchHandResetGameCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandResetGameCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();
        var info = component.UpdateHandInfo(req.ResetHandInfo);

        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandResetGameScRsp(info.Item1));
        }
        else
        {
            await connection.SendPacket(new PacketSwitchHandResetGameScRsp(info.Item2));
        }
    }
}