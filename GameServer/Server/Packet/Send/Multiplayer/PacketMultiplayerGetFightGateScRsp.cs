using EggLink.DanhengServer.GameServer.Game.MultiPlayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;

public class PacketMultiplayerGetFightGateScRsp : BasePacket
{
    public PacketMultiplayerGetFightGateScRsp(Retcode code) : base(CmdIds.MultiplayerGetFightGateScRsp)
    {
        var proto = new MultiplayerGetFightGateScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }

    public PacketMultiplayerGetFightGateScRsp(BaseMultiPlayerGameRoomInstance room) : base(
        CmdIds.MultiplayerGetFightGateScRsp)
    {
        var proto = new MultiplayerGetFightGateScRsp
        {
            GateRoomId = (ulong)room.RoomId,
            Ip = ConfigManager.Config.GameServer.PublicAddress,
            Port = ConfigManager.Config.GameServer.Port
        };

        SetData(proto);
    }
}