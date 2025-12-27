using EggLink.DanhengServer.GameServer.Game.MultiPlayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;

public class PacketMultiplayerFightGameStartScNotify : BasePacket
{
    public PacketMultiplayerFightGameStartScNotify(BaseMultiPlayerGameRoomInstance room) : base(
        CmdIds.MultiplayerFightGameStartScNotify)
    {
        var proto = new MultiplayerFightGameStartScNotify
        {
            SessionInfo = room.ToSessionInfo(),
            LobbyBasicInfo = { room.ParentLobby.Players.Select(x => x.ToProto()) }
        };

        SetData(proto);
    }
}