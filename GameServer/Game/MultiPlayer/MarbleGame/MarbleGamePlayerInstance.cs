using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.Lobby.Player;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;

public class MarbleGamePlayerInstance : BaseGamePlayerInstance
{
    public MarbleGamePlayerInstance(LobbyPlayerInstance lobby, MarbleTeamType type) : base(lobby)
    {
        TeamType = type;
        CurItemId = TeamType == MarbleTeamType.TeamA ? 200 : 300;

        var posXBaseValue = TeamType == MarbleTeamType.TeamA ? -1 : 1;
        var index = 0;
        foreach (var seal in lobby.EquippedSealList)
        {
            if (!GameData.MarbleSealData.TryGetValue(seal, out var marbleSeal)) continue;

            var posY = (index - 1) * 1.5f;
            var posX = posXBaseValue * (Math.Abs(index - 1) * 1 + 3);
            var rotX = posXBaseValue * -1f;
            AllowMoveSealList.Add(CurItemId);

            SealList.Add(CurItemId, new MarbleGameSealInstance(CurItemId++, seal)
            {
                Position = new MarbleSealVector
                {
                    X = posX,
                    Y = posY
                },
                Rotation = new MarbleSealVector
                {
                    X = rotX
                },
                Attack = marbleSeal.Attack,
                CurHp = marbleSeal.Hp,
                MaxHp = marbleSeal.Hp,
                Size = marbleSeal.Size,
                Mass = marbleSeal.Mass,
                MaxSpeed = marbleSeal.MaxSpeed
            });

            index++;
        }
    }

    public Dictionary<int, MarbleGameSealInstance> SealList { get; set; } = [];
    public MarbleTeamType TeamType { get; set; }
    public MarblePlayerPhaseEnum Phase { get; set; } = MarblePlayerPhaseEnum.NotEnter;
    public int CurItemId { get; set; }
    public int Score { get; set; }
    public HashSet<int> AllowMoveSealList { get; set; } = [];

    public void ChangeRound()
    {
        foreach (var instance in SealList.Values) AllowMoveSealList.Add(instance.Id);
    }
}