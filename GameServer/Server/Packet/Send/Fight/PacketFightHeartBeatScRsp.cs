using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightHeartBeatScRsp : BasePacket
{
    public PacketFightHeartBeatScRsp(ulong clientTime) : base(CmdIds.FightHeartBeatScRsp)
    {
        var proto = new FightHeartBeatScRsp
        {
            ServerTimeMs = (ulong)Extensions.GetUnixMs(),
            ClientTimeMs = clientTime
        };

        SetData(proto);
    }
}