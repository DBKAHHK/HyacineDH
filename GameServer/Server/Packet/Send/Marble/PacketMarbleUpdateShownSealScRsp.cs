using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleUpdateShownSealScRsp : BasePacket
{
    public PacketMarbleUpdateShownSealScRsp(ICollection<uint> sealList) : base(CmdIds.MarbleUpdateShownSealScRsp)
    {
        var proto = new MarbleUpdateShownSealScRsp
        {
            UpdateSealList = { sealList }
        };

        SetData(proto);
    }
}