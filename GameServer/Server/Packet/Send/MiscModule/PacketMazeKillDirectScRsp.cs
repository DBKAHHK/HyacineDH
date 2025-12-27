using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.MiscModule;

public class PacketMazeKillDirectScRsp : BasePacket
{
    public PacketMazeKillDirectScRsp(List<uint> entityIds) : base(CmdIds.MazeKillDirectScRsp)
    {
        var proto = new MazeKillDirectScRsp
        {
            EntityList = { entityIds }
        };

        SetData(proto);
    }
}