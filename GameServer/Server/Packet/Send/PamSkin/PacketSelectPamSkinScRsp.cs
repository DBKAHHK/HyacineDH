using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.PamSkin;

public class PacketSelectPamSkinScRsp : BasePacket
{
    public PacketSelectPamSkinScRsp(PlayerInstance player, int prevSkinId) : base(CmdIds.SelectPamSkinScRsp)
    {
        var proto = new SelectPamSkinScRsp
        {
            CurSkin = (uint)player.Data.CurrentPamSkin,
            SetSkin = (uint)prevSkinId
        };

        SetData(proto);
    }
}