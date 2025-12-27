using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public class MarbleGameFinishSyncData(MarbleGamePlayerInstance player, bool isWin)
    : MarbleGameBaseSyncData(MarbleNetWorkMsgEnum.GameFinish)
{
    public override FightGameInfo ToProto()
    {
        return new FightGameInfo
        {
            GameMessageType = (uint)MessageType,
            RogueFinishInfo = new MarbleGameFinishInfo
            {
                IsWin = isWin,
                SealOwnerUid = (uint)player.LobbyPlayer.Player.Uid,
                SealFinishInfoList =
                {
                    player.SealList.Keys.Select(x => new MarbleSealFinishInfo
                    {
                        ItemId = (uint)x,
                        MatchTitleId = 2
                    })
                }
            }
        };
    }
}