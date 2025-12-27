using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public abstract class MarbleGameBaseSyncData(MarbleNetWorkMsgEnum type)
{
    public MarbleNetWorkMsgEnum MessageType { get; set; } = type;
    public abstract FightGameInfo ToProto();
}