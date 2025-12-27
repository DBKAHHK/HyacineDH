using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Phone;

public class PacketGetPhoneDataScRsp : BasePacket
{
    public PacketGetPhoneDataScRsp(PlayerInstance player) : base(CmdIds.GetPhoneDataScRsp)
    {
        var proto = new GetPhoneDataScRsp
        {
            CurChatBubble = (uint)player.Data.ChatBubble,
            CurPhoneTheme = (uint)player.Data.PhoneTheme,
            CurPhoneCase = (uint)player.Data.PhoneCase
        };

        foreach (var item in player.PlayerUnlockData!.PhoneThemes) proto.OwnedPhoneThemes.Add((uint)item);
        foreach (var item in player.PlayerUnlockData!.ChatBubbles) proto.OwnedChatBubbles.Add((uint)item);
        foreach (var item in player.PlayerUnlockData!.PhoneCases) proto.OwnedPhoneCases.Add((uint)item);

        SetData(proto);
    }
}