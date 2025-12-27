using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandCoinUpdateScRsp : BasePacket
{
    public PacketSwitchHandCoinUpdateScRsp(Retcode ret) : base(CmdIds.SwitchHandCoinUpdateScRsp)
    {
        var proto = new SwitchHandCoinUpdateScRsp
        {
            Retcode = (uint)ret
        };
        SetData(proto);
    }
    public PacketSwitchHandCoinUpdateScRsp(uint coinNum) : base(CmdIds.SwitchHandCoinUpdateScRsp)
    {
        var proto = new SwitchHandCoinUpdateScRsp
        {
            HandCoinNum = coinNum
        };
        SetData(proto);
    }
}