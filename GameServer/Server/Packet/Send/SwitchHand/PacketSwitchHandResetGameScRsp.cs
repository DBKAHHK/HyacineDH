using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandResetGameScRsp : BasePacket
{
    public PacketSwitchHandResetGameScRsp(SwitchHandInfo info) : base(CmdIds.SwitchHandResetGameScRsp)
    {
        var proto = new SwitchHandResetGameScRsp
        {
            TargetHandInfo = info.ToProto()
        };

        SetData(proto);
    }
    public PacketSwitchHandResetGameScRsp(Retcode ret) : base(CmdIds.SwitchHandResetGameScRsp)
    {
        var proto = new SwitchHandResetGameScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}