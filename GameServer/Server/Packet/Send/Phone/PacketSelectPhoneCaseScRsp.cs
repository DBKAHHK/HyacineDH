using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Phone;

public class PacketSelectPhoneCaseScRsp : BasePacket
{
    public PacketSelectPhoneCaseScRsp(uint id) : base(CmdIds.SelectPhoneCaseScRsp)
    {
        var proto = new SelectPhoneCaseScRsp
        {
            CurPhoneCase = id
        };

        SetData(proto);
    }
}