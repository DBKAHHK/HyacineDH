using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandResetHandPosCsReq)]
public class HandlerSwitchHandResetHandPosCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandResetHandPosCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();
        
        var info = component.GetHandInfo((int)req.ConfigId);
        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandResetHandPosScRsp(info.Item1));
        }
        else
        {
            info.Item2.Pos = req.HandMotion.Pos.ToPosition();
            info.Item2.Rot = req.HandMotion.Rot.ToPosition();

            await connection.SendPacket(new PacketSwitchHandResetHandPosScRsp(info.Item2));
        }
    }
}