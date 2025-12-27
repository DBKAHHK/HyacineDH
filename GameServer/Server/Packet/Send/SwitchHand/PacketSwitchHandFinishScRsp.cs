using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandFinishScRsp : BasePacket
{
    public PacketSwitchHandFinishScRsp(SwitchHandInfo info) : base(CmdIds.SwitchHandFinishScRsp)
    {
        var proto = new SwitchHandFinishScRsp
        {
            HandInfo = info.ToProto()
        };

        SetData(proto);
    }

    public PacketSwitchHandFinishScRsp(Retcode ret) : base(CmdIds.SwitchHandFinishScRsp)
    {
        var proto = new SwitchHandFinishScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}