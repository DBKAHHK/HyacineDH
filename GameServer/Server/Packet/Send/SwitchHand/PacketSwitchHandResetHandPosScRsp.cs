using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandResetHandPosScRsp : BasePacket
{
    public PacketSwitchHandResetHandPosScRsp(SwitchHandInfo info) : base(CmdIds.SwitchHandResetHandPosScRsp)
    {
        var proto = new SwitchHandResetHandPosScRsp
        {
            TargetHandInfo = info.ToProto()
        };

        SetData(proto);
    }

    public PacketSwitchHandResetHandPosScRsp(Retcode ret) : base(CmdIds.SwitchHandResetHandPosScRsp)
    {
        var proto = new SwitchHandResetHandPosScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}