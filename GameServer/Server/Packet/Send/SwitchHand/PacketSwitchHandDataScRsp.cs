using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandDataScRsp : BasePacket
{
    private static OHILHLOPGLN ConvertHandInfoToOHILHLOPGLN(HandInfo handInfo)
    {
        return new OHILHLOPGLN
        {
            // HNOEHOLMKPN 原名为 HandCoinNum
            HNOEHOLMKPN = handInfo.HandCoinNum,
            // GJONGBJCLIL 原名为 HandState
            GJONGBJCLIL = handInfo.HandState,
            // ConfigId 原名为 config_id
            ConfigId = handInfo.ConfigId,
            // DAFAKILIICA 原名为 HandByteValue
            DAFAKILIICA = handInfo.HandByteValue,
            // OABJKNADMLA 原名为 HandMotion
            OABJKNADMLA = handInfo.HandMotion
        };
    }

    public PacketSwitchHandDataScRsp(SwitchHandInfo info) : base(CmdIds.SwitchHandDataScRsp)
    {
        var proto = new SwitchHandDataScRsp
        {
            ELKBJCGABPP = { ConvertHandInfoToOHILHLOPGLN(info.ToProto()) }
        };

        SetData(proto);
    }

    public PacketSwitchHandDataScRsp(List<SwitchHandInfo> infos) : base(CmdIds.SwitchHandDataScRsp)
    {
        var proto = new SwitchHandDataScRsp
        {
            ELKBJCGABPP = { infos.Select(x => ConvertHandInfoToOHILHLOPGLN(x.ToProto())) }
        };

        SetData(proto);
    }

    public PacketSwitchHandDataScRsp(Retcode code) : base(CmdIds.SwitchHandDataScRsp)
    {
        var proto = new SwitchHandDataScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}