using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleLevelFinishScRsp : BasePacket
{
    public PacketMarbleLevelFinishScRsp(uint levelId) : base(CmdIds.MarbleLevelFinishScRsp)
    {
        var proto = new MarbleLevelFinishScRsp
        {
            MarbleLevelId = levelId
        };

        SetData(proto);
    }
}