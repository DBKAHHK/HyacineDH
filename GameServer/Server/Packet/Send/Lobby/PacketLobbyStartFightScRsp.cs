using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbyStartFightScRsp : BasePacket
{
    public PacketLobbyStartFightScRsp(Retcode code) : base(CmdIds.LobbyStartFightScRsp)
    {
        var proto = new LobbyStartFightScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}