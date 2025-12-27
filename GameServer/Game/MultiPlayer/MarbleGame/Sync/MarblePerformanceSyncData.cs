using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public class MarblePerformanceSyncData(MarbleNetWorkMsgEnum type) : MarbleGameBaseSyncData(type)
{
    public override FightGameInfo ToProto()
    {
        return new FightGameInfo
        {
            GameMessageType = (uint)MessageType,
            MarbleGameSyncInfo = new MarbleGameSyncInfo
            {
                MarbleSyncType = MarbleSyncType.Performance
            }
        };
    }
}