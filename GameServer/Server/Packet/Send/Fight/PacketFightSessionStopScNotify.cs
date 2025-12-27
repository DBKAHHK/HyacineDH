using EggLink.DanhengServer.GameServer.Game.MultiPlayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightSessionStopScNotify : BasePacket
{
    public PacketFightSessionStopScNotify(BaseMultiPlayerGameRoomInstance room) : base(CmdIds.FightSessionStopScNotify)
    {
        var proto = new FightSessionStopScNotify
        {
            // MBAPAPJPCNI 原名为 FightSessionInfo
            MBAPAPJPCNI = new BDMEPNIPOOM
            {
                // PELOGDHKILB 原名为 SessionRoomId
                PELOGDHKILB = (ulong)room.RoomId,
                // FMCMLOFEDCP 原名为 SessionGameMode
                FMCMLOFEDCP = room.GameMode
            }
        };

        SetData(proto);
    }
}