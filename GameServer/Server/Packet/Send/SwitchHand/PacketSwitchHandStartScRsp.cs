using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandStartScRsp : BasePacket
{
    public PacketSwitchHandStartScRsp(uint configId) : base(CmdIds.SwitchHandStartScRsp)
    {
        var proto = new SwitchHandStartScRsp
        {
            ConfigId = configId
        };

        SetData(proto);
    }
}