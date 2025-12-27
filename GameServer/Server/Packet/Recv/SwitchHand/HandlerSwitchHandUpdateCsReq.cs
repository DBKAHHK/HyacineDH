using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandUpdateCsReq)]
public class HandlerSwitchHandUpdateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandUpdateCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();
        var info = component.UpdateHandInfo(req.OperationHandInfo);
        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandUpdateScRsp(info.Item1, req.HandOperationInfo));
        }
        else
        {
            await connection.SendPacket(new PacketSwitchHandUpdateScRsp(info.Item2, req.HandOperationInfo));
        }
    }
}