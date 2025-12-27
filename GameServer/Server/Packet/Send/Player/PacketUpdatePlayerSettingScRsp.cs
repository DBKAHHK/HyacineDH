using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketUpdatePlayerSettingScRsp : BasePacket
{
    public PacketUpdatePlayerSettingScRsp(UpdatePlayerSetting setting) : base(CmdIds.UpdatePlayerSettingScRsp)
    {
        var proto = new UpdatePlayerSettingScRsp
        {
            PlayerSetting = setting
        };

        SetData(proto);
    }
}