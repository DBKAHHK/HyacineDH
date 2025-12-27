using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Gacha;

public class PacketSetGachaDecideItemScRsp : BasePacket
{
    public PacketSetGachaDecideItemScRsp(uint gachaId, List<uint> order) : base(CmdIds.SetGachaDecideItemScRsp)
    {
        var proto = new SetGachaDecideItemScRsp
        {
            // DCJPKILIKOM 原名为 DecideItemInfo
            DCJPKILIKOM = new AOEOIAOCLJF
            {
                // DPKOPFDEFMN 原名为 DecideItemOrder
                DPKOPFDEFMN = { order },
                // CHMMPLOFCPN 原名为 CHDOIBFEHLP
                CHMMPLOFCPN = 1,
                // GIIHKBCOGOB 原名为 JIGONEALCPC
                GIIHKBCOGOB = { 11 }
            }
        };

        SetData(proto);
    }
}