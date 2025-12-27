using EggLink.DanhengServer.GameServer.Game.MultiPlayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;

public class PacketMultiplayerFightGameFinishScNotify : BasePacket
{
    public PacketMultiplayerFightGameFinishScNotify(BaseMultiPlayerGameRoomInstance room) : base(
        CmdIds.MultiplayerFightGameFinishScNotify)
    {
        var proto = new MultiplayerFightGameFinishScNotify
        {
            SessionInfo = new FightSessionInfo
            {
                SessionGameMode = room.GameMode,
                SessionRoomId = (ulong)room.RoomId
            }
        };

        SetData(proto);
    }
}