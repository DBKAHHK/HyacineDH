using EggLink.DanhengServer.GameServer.Game.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbySyncInfoScNotify : BasePacket
{
    public PacketLobbySyncInfoScNotify(int uid, LobbyRoomInstance room, LobbyModifyType modifyType) : base(
        CmdIds.LobbySyncInfoScNotify)
    {
        var proto = new LobbySyncInfoScNotify
        {
            LobbyBasicInfo = { room.Players.Select(x => x.ToProto()) },
            Uid = (uint)uid,
            Type = modifyType
        };

        SetData(proto);
    }
}