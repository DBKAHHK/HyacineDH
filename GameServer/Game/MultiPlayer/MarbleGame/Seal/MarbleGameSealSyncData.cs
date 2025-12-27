using System.Numerics;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;

public abstract class BaseMarbleGameSyncData
{
    public abstract MarbleGameSyncData ToProto();
}

public class MarbleGameScoreSyncData(int playerAScore, int playerBScore, MarbleFrameType frameType)
    : BaseMarbleGameSyncData
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            FrameType = frameType,
            PlayerAScore = (uint)playerAScore,
            PlayerBScore = (uint)playerBScore
        };
    }
}

public class MarbleGameHpChangeSyncData(
    MarbleGameSealInstance inst,
    MarbleFrameType frameType,
    int changeValue,
    float time = 0f) : MarbleGameSealSyncData(inst, frameType)
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            FrameType = FrameType,
            Id = (uint)Instance.Id,
            Time = time,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            HpChangeValue = changeValue,
            Attack = Instance.Attack
        };
    }
}

public class MarbleGameEffectSyncData(
    MarbleGameSealInstance inst,
    MarbleFrameType frameType,
    int skillId,
    float time = 0f) : MarbleGameSealSyncData(inst, frameType)
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            FrameType = FrameType,
            Id = (uint)Instance.Id,
            Time = time,
            SealSkillId = (uint)skillId,
            CollisionPosition = new MarbleSealVector()
        };
    }
}

public class MarbleGameSealSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType) : BaseMarbleGameSyncData
{
    public MarbleGameSealInstance Instance { get; set; } = inst.Clone();
    public MarbleFrameType FrameType { get; set; } = frameType;

    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            Attack = Instance.Attack,
            Id = (uint)Instance.Id,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            SealPosition = Instance.Position,
            SealRotation = Instance.Rotation,
            SealOnStage = Instance.OnStage,
            SealSize = Instance.Size,
            FrameType = FrameType
        };
    }
}

public class MarbleGameSealActionSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType, float time = 0)
    : MarbleGameSealSyncData(inst, frameType)
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            Attack = Instance.Attack,
            Id = (uint)Instance.Id,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            SealPosition = Instance.Position,
            SealRotation = Instance.Rotation,
            SealOnStage = Instance.OnStage,
            SealSize = Instance.Size,
            FrameType = FrameType,
            Time = time
        };
    }
}

public class MarbleGameSealLaunchStopSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType, float time = 0)
    : MarbleGameSealSyncData(inst, frameType)
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            Attack = Instance.Attack,
            Id = (uint)Instance.Id,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            SealPosition = Instance.Position,
            SealRotation = Instance.Rotation,
            FrameType = FrameType,
            Time = time,
            SealVelocity = Instance.Velocity
        };
    }
}

public class MarbleGameSealCollisionSyncData(
    MarbleGameSealInstance inst,
    int collideOwnerId,
    int collideTargetId,
    float time,
    Vector2 collidePos,
    MarbleSealVector? targetVelocity) : MarbleGameSealSyncData(inst, MarbleFrameType.Collide)
{
    public override MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            Attack = Instance.Attack,
            Id = (uint)Instance.Id,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            SealPosition = Instance.Position,
            SealRotation = Instance.Rotation,
            FrameType = MarbleFrameType.Collide,
            CollideType = collideTargetId == 1 ? MarbleFactionType.Field :
                collideTargetId / 100 == collideOwnerId / 100 ? MarbleFactionType.Ally : MarbleFactionType.Enemy,
            CollideOwnerId = (uint)collideOwnerId,
            CollideTargetId = (uint)collideTargetId,
            CollisionPosition = new MarbleSealVector
            {
                X = collidePos.X,
                Y = collidePos.Y
            },
            CollisionTargetVelocity = targetVelocity ?? new MarbleSealVector(),
            SealVelocity = Instance.Velocity,
            Time = time
        };
    }
}