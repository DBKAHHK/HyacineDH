using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbyModifyPlayerInfoScRsp : BasePacket
{
    public PacketLobbyModifyPlayerInfoScRsp(Retcode code) : base(CmdIds.LobbyModifyPlayerInfoScRsp)
    {
        var proto = new LobbyModifyPlayerInfoScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}