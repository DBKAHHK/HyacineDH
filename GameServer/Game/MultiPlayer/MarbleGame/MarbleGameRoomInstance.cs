using System.Numerics;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.Lobby;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Physics;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;
using EggLink.DanhengServer.GameServer.Server;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;

public class MarbleGameRoomInstance : BaseMultiPlayerGameRoomInstance
{
    public MarbleGameRoomInstance(long roomId, LobbyRoomInstance parentLobby) : base(roomId, parentLobby)
    {
        // random move team type
        CurMoveTeamType = (MarbleTeamType)Random.Shared.Next(1, 3);
        FirstMoveTeamType = CurMoveTeamType;
        // set player
        foreach (var player in parentLobby.Players)
            Players.Add(new MarbleGamePlayerInstance(player, (MarbleTeamType)(parentLobby.Players.IndexOf(
                player) + 1)));
    }

    public MarbleTeamType CurMoveTeamType { get; set; }
    public MarbleTeamType FirstMoveTeamType { get; set; }
    public int CurRound { get; set; }
    public int TurnCount { get; set; }
    public long WaitingOperationEndTime { get; set; }

    public MarbleGameInfo ToProto()
    {
        return new MarbleGameInfo
        {
            LobbyBasicInfo = { ParentLobby.Players.Select(x => x.ToProto()) },
            CurActionTeamType = CurMoveTeamType,
            LevelId = 100,
            TeamAPlayer = (uint)Players[0].LobbyPlayer.Player.Uid,
            TeamBPlayer = (uint)Players[1].LobbyPlayer.Player.Uid,
            TeamARank = 1,
            TeamBRank = 1,
            TeamASealList = { (Players[0] as MarbleGamePlayerInstance)!.SealList.Select(x => (uint)x.Value.SealId) },
            TeamBSealList = { (Players[1] as MarbleGamePlayerInstance)!.SealList.Select(x => (uint)x.Value.SealId) },
            PlayerAScore = (uint)(Players[0] as MarbleGamePlayerInstance)!.Score,
            PlayerBScore = (uint)(Players[1] as MarbleGamePlayerInstance)!.Score,
            ControlByServer = true
        };
    }


    #region Player

    public async ValueTask BroadCastToRoom(BasePacket packet)
    {
        foreach (var player in Players) await player.SendPacket(packet);
    }

    public async ValueTask BroadCastToRoomPlayer(BasePacket packet)
    {
        foreach (var player in Players.Where(x => !x.LeaveGame)) await player.LobbyPlayer.Player.SendPacket(packet);
    }

    public async ValueTask EnterGame(int uid)
    {
        var player = GetPlayerById(uid);
        if (player == null) return;
        player.EnterGame = true;

        if (player is MarbleGamePlayerInstance marblePlayer) marblePlayer.Phase = MarblePlayerPhaseEnum.EnterGame;

        if (Players.All(x => x.EnterGame))
            // send basic info
            await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
                MarbleNetWorkMsgEnum.GameStart, this));
    }

    public async ValueTask OnPlayerHeartBeat()
    {
        var curTime = Extensions.GetUnixMs();
        if (WaitingOperationEndTime > 0 && curTime >= WaitingOperationEndTime)
            // timeout
            await SwitchTurn();
    }

    public List<MarbleGameBaseSyncData> CheckPlayerWin()
    {
        var winPlayer = Players.OfType<MarbleGamePlayerInstance>().FirstOrDefault(x => x.Score >= 6);
        if (winPlayer == null) return [];

        List<MarbleGameBaseSyncData> syncData = [];
        // win
        foreach (var player in Players.OfType<MarbleGamePlayerInstance>())
            syncData.Add(new MarbleGameFinishSyncData(player, player == winPlayer));

        return syncData;
    }

    public async ValueTask EndGame()
    {
        // end game
        await BroadCastToRoom(new PacketFightSessionStopScNotify(this));
        await BroadCastToRoom(new PacketMultiplayerFightGameFinishScNotify(this));
        foreach (var marblePlayer in Players.OfType<MarbleGamePlayerInstance>())
        {
            await marblePlayer.LobbyPlayer.LobbyRoom.EndFight(marblePlayer.LobbyPlayer);
            marblePlayer.Connection?.Stop();
        }

        // remove room
        ServerUtils.MultiPlayerGameServerManager.RemoveRoom(RoomId);
    }

    #endregion

    #region Handler

    public async ValueTask HandleGeneralRequest(MarbleGamePlayerInstance player, uint msgType, byte[] reqData)
    {
        var messageType = (MarbleNetWorkMsgEnum)msgType;

        switch (messageType)
        {
            case MarbleNetWorkMsgEnum.LoadFinish:
                await LoadFinish(player);
                break;
            case MarbleNetWorkMsgEnum.PerformanceFinish:
                await PerformanceFinish(player);
                break;
            case MarbleNetWorkMsgEnum.Launch:
                var req = MarbleGameLaunchInfo.Parser.ParseFrom(reqData);
                await HandleLaunch((int)req.ItemId, new Vector2(req.SealTargetRotation.X, req.SealTargetRotation.Y));
                break;
            case MarbleNetWorkMsgEnum.Operation:
                var operationReq = FightMarbleSealInfo.Parser.ParseFrom(reqData);
                operationReq.SealOwnerUid = (uint)player.LobbyPlayer.Player.Uid;
                await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
                    MarbleNetWorkMsgEnum.Operation, operationReq));
                break;
            case MarbleNetWorkMsgEnum.SimulateFinish:
                await HandleSimulateFinish(player);
                break;
        }
    }

    public async ValueTask LoadFinish(MarbleGamePlayerInstance player)
    {
        player.Phase = MarblePlayerPhaseEnum.LoadFinish;
        if (Players.OfType<MarbleGamePlayerInstance>().ToList().Where(x => !x.LeaveGame)
            .All(x => x.Phase == MarblePlayerPhaseEnum.LoadFinish))
            // next phase (performance)
            await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
                [new MarblePerformanceSyncData(MarbleNetWorkMsgEnum.SyncNotify)]));
    }

    public async ValueTask PerformanceFinish(MarbleGamePlayerInstance player)
    {
        player.Phase = MarblePlayerPhaseEnum.PerformanceFinish;
        if (Players.OfType<MarbleGamePlayerInstance>().ToList().Where(x => !x.LeaveGame)
            .All(x => x.Phase == MarblePlayerPhaseEnum.PerformanceFinish))
            // next phase (round start)
            await RoundStart();
    }

    public async ValueTask HandleSimulateFinish(MarbleGamePlayerInstance player)
    {
        player.Phase = MarblePlayerPhaseEnum.SimulateFinish;
        if (Players.OfType<MarbleGamePlayerInstance>().ToList().Where(x => !x.LeaveGame)
            .All(x => x.Phase == MarblePlayerPhaseEnum.SimulateFinish))
            // switch turn
            await SwitchTurn();
    }

    #endregion

    #region Round

    public async ValueTask RoundStart()
    {
        CurRound++;
        TurnCount = 0;
        CurMoveTeamType = FirstMoveTeamType;
        foreach (var player in Players.OfType<MarbleGamePlayerInstance>()) player.ChangeRound();

        await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
        [
            new MarbleGameInfoSyncData(MarbleNetWorkMsgEnum.SyncNotify, MarbleSyncType.RoundStart, this,
                Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                    .Select(x => new MarbleGameSealSyncData(x, MarbleFrameType.RoundStart)).ToList())
        ]));

        WaitingOperationEndTime = Extensions.GetUnixMs() + 1000 * 30;
        TurnCount++;
    }

    public async ValueTask SwitchTurn()
    {
        var moveCount = Players.OfType<MarbleGamePlayerInstance>()
            .SelectMany(x => x.SealList.Values.Where(j => j.OnStage)).Count();

        if (moveCount == TurnCount)
        {
            await RoundEnd();
            return;
        }

        CurMoveTeamType = CurMoveTeamType == MarbleTeamType.TeamA ? MarbleTeamType.TeamB : MarbleTeamType.TeamA;

        foreach (var player in Players.OfType<MarbleGamePlayerInstance>()) player.Phase = MarblePlayerPhaseEnum.Gaming;

        await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
        [
            new MarbleGameInfoSyncData(MarbleNetWorkMsgEnum.SyncNotify, MarbleSyncType.SwitchRound, this,
                Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                    .Select(x => new MarbleGameSealSyncData(x, MarbleFrameType.RoundStart)).ToList())
        ]));

        WaitingOperationEndTime = Extensions.GetUnixMs() + 1000 * 30;
        TurnCount++;
    }

    public async ValueTask RoundEnd()
    {
        FirstMoveTeamType = FirstMoveTeamType == MarbleTeamType.TeamA ? MarbleTeamType.TeamB : MarbleTeamType.TeamA;

        foreach (var player in Players.OfType<MarbleGamePlayerInstance>()) player.Phase = MarblePlayerPhaseEnum.Gaming;

        await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
        [
            new MarbleGameInfoSyncData(MarbleNetWorkMsgEnum.SyncNotify, MarbleSyncType.RoundEnd, this,
                Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                    .Select(x => new MarbleGameSealSyncData(x, MarbleFrameType.RoundEnd)).ToList())
        ]));

        WaitingOperationEndTime = 0;

        await RoundStart();
    }

    #endregion

    #region Collision

    public async ValueTask HandleLaunch(int itemId, Vector2 rotation)
    {
        WaitingOperationEndTime = 0;

        var player = Players.OfType<MarbleGamePlayerInstance>().FirstOrDefault(x => x.SealList.ContainsKey(itemId));
        if (player == null) return;
        var seal = player.SealList[itemId];
        if (!GameData.MarbleSealData.TryGetValue(seal.SealId, out var sealExcel)) return;

        player.AllowMoveSealList.Remove(seal.Id);

        var speed = sealExcel.MaxSpeed * rotation;
        var simulator = new CollisionSimulator(
            -5.25f,
            5.25f,
            3f,
            -3f
        )
        {
            LaunchTeam = itemId / 100
        };

        seal.Velocity = new MarbleSealVector
        {
            X = speed.X,
            Y = speed.Y
        };

        List<BaseMarbleGameSyncData> syncData =
        [
            new MarbleGameEffectSyncData(seal, MarbleFrameType.Effect, 101)
        ];

        foreach (var sealInst in Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                     .Where(x => x.OnStage))
            simulator.AddBall(sealInst.Id, new Vector2(sealInst.Position.X, sealInst.Position.Y), sealInst.Mass,
                sealInst.Size, new Vector2(sealInst.Velocity.X, sealInst.Velocity.Y), hp: sealInst.CurHp,
                atk: sealInst.Attack);

        syncData.AddRange(Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
            .Select(sealInst => new MarbleGameSealActionSyncData(sealInst, MarbleFrameType.ActionStart)));

        syncData.Add(new MarbleGameSealLaunchStopSyncData(seal, MarbleFrameType.Launch));
        simulator.Simulate();

        foreach (var recordRaw in simulator.Records) // process record
            switch (recordRaw)
            {
                case CollisionRecord record:
                {
                    var sealInst = Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                        .FirstOrDefault(x => x.Id == record.BallA.Id);
                    if (sealInst == null) continue;
                    var sealInstB = Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                        .FirstOrDefault(x => x.Id == (record.BallB?.Id ?? 1));

                    sealInst.Velocity = new MarbleSealVector
                    {
                        X = record.BallA.Velocity.X,
                        Y = record.BallA.Velocity.Y
                    };

                    sealInst.Position = new MarbleSealVector
                    {
                        X = record.BallA.Position.X,
                        Y = record.BallA.Position.Y
                    };

                    if (record.BallA.Velocity != Vector2.Zero) // avoid zero vector
                    {
                        var velocityANormal = Vector2.Normalize(record.BallA.Velocity);
                        sealInst.Rotation = new MarbleSealVector
                        {
                            X = velocityANormal.X,
                            Y = velocityANormal.Y
                        };
                    }

                    if (sealInstB != null && record.BallB != null)
                    {
                        var velocityBNormal = Vector2.Normalize(record.BallB.Velocity);
                        if (record.BallB.Velocity != Vector2.Zero)
                            sealInstB.Rotation = new MarbleSealVector
                            {
                                X = velocityBNormal.X,
                                Y = velocityBNormal.Y
                            };

                        sealInstB.Velocity = new MarbleSealVector
                        {
                            X = record.BallB.Velocity.X,
                            Y = record.BallB.Velocity.Y
                        };

                        sealInstB.Position = new MarbleSealVector
                        {
                            X = record.BallB.Position.X,
                            Y = record.BallB.Position.Y
                        };

                        if (sealInstB.Id / 100 != sealInst.Id / 100)
                            // different teams
                            // do damage to b
                            syncData.AddRange(sealInst.Id / 100 == itemId / 100
                                ? DoDamage(sealInst, sealInstB, record.Time)
                                // do damage to a
                                : DoDamage(sealInstB, sealInst, record.Time));
                    }

                    syncData.Add(new MarbleGameSealCollisionSyncData(sealInst, record.BallA.Id, record.BallB?.Id ?? 1,
                        record.Time, record.CollisionPos, sealInstB?.Velocity));
                    if (sealInstB != null)
                        syncData.Add(new MarbleGameSealCollisionSyncData(sealInstB, record.BallA.Id,
                            record.BallB?.Id ?? 1,
                            record.Time, record.CollisionPos, sealInst.Velocity));

                    break;
                }
                case StopRecord stopRecord:
                {
                    var sealInst = Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                        .FirstOrDefault(x => x.Id == stopRecord.Ball.Id);
                    if (sealInst == null) continue;
                    sealInst.Velocity = new MarbleSealVector
                    {
                        X = 0,
                        Y = 0
                    };

                    sealInst.Position = new MarbleSealVector
                    {
                        X = stopRecord.Ball.Position.X,
                        Y = stopRecord.Ball.Position.Y
                    };

                    syncData.Add(new MarbleGameSealLaunchStopSyncData(sealInst, MarbleFrameType.Stop, stopRecord.Time));
                    break;
                }
                case ChangeSpeedRecord changeSpeedRecord:
                {
                    var sealInst = Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                        .FirstOrDefault(x => x.Id == changeSpeedRecord.Ball.Id);
                    if (sealInst == null) continue;
                    sealInst.Velocity = new MarbleSealVector
                    {
                        X = changeSpeedRecord.Ball.Velocity.X,
                        Y = changeSpeedRecord.Ball.Velocity.Y
                    };

                    sealInst.Position = new MarbleSealVector
                    {
                        X = changeSpeedRecord.Ball.Position.X,
                        Y = changeSpeedRecord.Ball.Position.Y
                    };

                    if (changeSpeedRecord.Ball.Velocity != Vector2.Zero)
                    {
                        var normal = Vector2.Normalize(changeSpeedRecord.Ball.Velocity);
                        sealInst.Rotation = new MarbleSealVector
                        {
                            X = normal.X,
                            Y = normal.Y
                        };
                    }

                    syncData.Add(new MarbleGameSealLaunchStopSyncData(sealInst, MarbleFrameType.ChangeSpeed,
                        changeSpeedRecord.Time));
                    break;
                }
            }

        foreach (var ball in simulator.Balls)
        {
            var sealInst = Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                .FirstOrDefault(x => x.Id == ball.Id);
            if (sealInst == null) continue;

            sealInst.Position = new MarbleSealVector
            {
                X = ball.Position.X,
                Y = ball.Position.Y
            };

            sealInst.Velocity = new MarbleSealVector
            {
                X = ball.Velocity.X,
                Y = ball.Velocity.Y
            };
        }

        syncData.AddRange(Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
            .Select(sealInst =>
                new MarbleGameSealActionSyncData(sealInst, MarbleFrameType.ActionEnd, simulator.CurTime)));

        syncData.AddRange(ReviveAllDeadSeals(simulator.CurTime));
        var winData = CheckPlayerWin();

        await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
        [
            new MarbleGameInfoLaunchingSyncData(MarbleNetWorkMsgEnum.SyncNotify, MarbleSyncType.SimulateStart,
                simulator.CurTime, itemId, this, syncData),
            ..winData
        ]));

        foreach (var p in Players.OfType<MarbleGamePlayerInstance>()) p.Phase = MarblePlayerPhaseEnum.Launching;

        if (winData.Count > 0) await EndGame();
    }

    public List<BaseMarbleGameSyncData> DoDamage(MarbleGameSealInstance attacker, MarbleGameSealInstance target,
        float time)
    {
        List<BaseMarbleGameSyncData> syncData = [];
        var attackerPlayer = Players.OfType<MarbleGamePlayerInstance>().FirstOrDefault(x =>
            x.SealList.ContainsKey(attacker.Id));
        var targetPlayer = Players.OfType<MarbleGamePlayerInstance>().FirstOrDefault(x =>
            x.SealList.ContainsKey(target.Id));

        if (attackerPlayer == null || targetPlayer == null) return syncData;
        var damage = attacker.Attack;
        target.CurHp -= damage;
        syncData.Add(new MarbleGameHpChangeSyncData(target, MarbleFrameType.HpChange, -damage, time));
        if (target.CurHp <= 0)
        {
            // die
            target.CurHp = 0;
            // score
            if (Players.OfType<MarbleGamePlayerInstance>().All(x => x.Score < 6))
            {
                attackerPlayer.Score++;
                syncData.Add(new MarbleGameScoreSyncData((Players[0] as MarbleGamePlayerInstance)!.Score,
                    (Players[1] as MarbleGamePlayerInstance)!.Score, MarbleFrameType.TeamScore));
            }
        }

        return syncData;
    }

    public List<BaseMarbleGameSyncData> ReviveAllDeadSeals(float time)
    {
        List<BaseMarbleGameSyncData> syncData = [];
        var seals = Players.OfType<MarbleGamePlayerInstance>()
            .SelectMany(x => x.SealList.Values).ToList();

        var deadSeals = seals.Where(j => j.CurHp <= 0).ToList();
        if (deadSeals.Count == 0) return syncData;

        foreach (var seal in deadSeals)
        {
            var player = Players.OfType<MarbleGamePlayerInstance>()
                .FirstOrDefault(x => x.SealList.ContainsKey(seal.Id));
            if (player == null) continue;

            var posXBaseValue = player.TeamType == MarbleTeamType.TeamA ? -1 : 1;
            var index = seal.Id % 100;
            var posY = (index - 1) * 1.5f;
            var posX = posXBaseValue * (Math.Abs(index - 1) * 1 + 3);
            seal.CurHp = seal.MaxHp;
            seal.OnStage = true;
            seal.Velocity = new MarbleSealVector();

            seal.Position = new MarbleSealVector
            {
                X = posX,
                Y = posY
            };

            seal.Rotation = new MarbleSealVector
            {
                X = posXBaseValue * -1f
            };

            syncData.RemoveAll(x => x.ToProto().Id == seal.Id);
            syncData.Add(new MarbleGameSealActionSyncData(seal, MarbleFrameType.Revive, time));
        }

        bool anyMove;
        do
        {
            // detect collision with seals
            anyMove = false;
            for (var i = 0; i < seals.Count; i++)
            for (var j = i + 1; j < seals.Count; j++)
            {
                var sealA = seals[i];
                var sealB = seals[j];

                var sealAPos = new Vector2(sealA.Position.X, sealA.Position.Y);
                var sealBPos = new Vector2(sealB.Position.X, sealB.Position.Y);
                if (!(Vector2.Distance(sealBPos, sealAPos) <= sealA.Size + sealB.Size)) continue;

                anyMove = true;
                // move sealB away
                var normalVec = Vector2.Normalize(sealBPos - sealAPos);
                var moveDistance = sealA.Size + sealB.Size - Vector2.Distance(sealBPos, sealAPos) + 0.1f;
                var moveVec = normalVec * moveDistance;
                sealB.Position = new MarbleSealVector
                {
                    X = sealB.Position.X + moveVec.X,
                    Y = sealB.Position.Y + moveVec.Y
                };

                syncData.RemoveAll(x => x.ToProto().Id == sealB.Id);
                syncData.Add(new MarbleGameSealActionSyncData(sealB, MarbleFrameType.Revive, time));
            }
        } while (anyMove);

        return syncData;
    }

    #endregion
}