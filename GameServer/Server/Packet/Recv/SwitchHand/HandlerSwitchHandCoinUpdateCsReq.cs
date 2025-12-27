using EggLink.DanhengServer.GameServer.Game.Player.Components;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandCoinUpdateCsReq)]
public class HandlerSwitchHandCoinUpdateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandCoinUpdateCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();
        var info = component.GetHandInfo(component.RunningHandConfigId);
        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandCoinUpdateScRsp(info.Item1));
        }
        else
        {
            info.Item2.CoinNum = (int)req.HandCoinNum;
            await connection.SendPacket(new PacketSwitchHandCoinUpdateScRsp(req.HandCoinNum));
        }
    }
}