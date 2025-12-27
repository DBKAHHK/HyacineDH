using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Chat;

public class PacketRevcMsgScNotify : BasePacket
{
    public PacketRevcMsgScNotify(uint toUid, uint fromUid, string msg) : base(CmdIds.RevcMsgScNotify)
    {
        var proto = new RevcMsgScNotify
        {
            ChatType = ChatType.Private,
            SourceUid = fromUid,
            TargetUid = toUid,
            MessageText = msg,
            MessageType = MsgType.CustomText
        };

        SetData(proto);
    }

    public PacketRevcMsgScNotify(uint toUid, uint fromUid, uint extraId) : base(CmdIds.RevcMsgScNotify)
    {
        var proto = new RevcMsgScNotify
        {
            ChatType = ChatType.Private,
            SourceUid = fromUid,
            TargetUid = toUid,
            ExtraId = extraId,
            MessageType = MsgType.Emoji
        };

        SetData(proto);
    }

    public PacketRevcMsgScNotify(uint toUid, uint fromUid, LobbyInviteInfo info) : base(CmdIds.RevcMsgScNotify)
    {
        var proto = new RevcMsgScNotify
        {
            ChatType = ChatType.Private,
            SourceUid = fromUid,
            TargetUid = toUid,
            // DIJGJPGFFLG 是消息内容，原名为 LobbyInviteInfo
            DIJGJPGFFLG = new AJEMANPKNHC
            {
                // PAJIMKAGAPI 原名为 FightGameInviteInfo
                PAJIMKAGAPI = new IDKBGCOJHHJ
                {
                    // COPGBEFIKGE 原名为 InviteRoomId
                    COPGBEFIKGE = info.FightGameInviteInfo?.InviteRoomId ?? 0,
                    // GCLCCOGIDOA 原名为 FightGameMode
                    GCLCCOGIDOA = info.FightGameInviteInfo?.FightGameMode ?? 0,
                    // GBAHHHPMLDK 原名为 AHBEMDLGGEO
                    GBAHHHPMLDK = info.FightGameInviteInfo?.AHBEMDLGGEO ?? 0,
                    // LCHNDBNKNIO 原名为其他属性
                    LCHNDBNKNIO = { }
                },
                // CJPMKIJMPOE 原名为 CELMKOLBJNN
                CJPMKIJMPOE = new BMEMLBHJDMJ
                {
                    // PanelId 保持原名
                    PanelId = info.CELMKOLBJNN?.PanelId ?? 0,
                    // ModifierContentType 保持原名
                    ModifierContentType = info.CELMKOLBJNN?.ModifierContentType ?? 0,
                    // OBIFCELHINM 原名为 CFDANMOMHPI
                    OBIFCELHINM = info.CELMKOLBJNN?.CFDANMOMHPI ?? 0
                }
            },
            MessageType = MsgType.Invite
        };

        SetData(proto);
    }
}