using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbyQuitScRsp : BasePacket
{
    public PacketLobbyQuitScRsp(Retcode code) : base(CmdIds.LobbyQuitScRsp)
    {
        var proto = new LobbyModifyPlayerInfoScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}