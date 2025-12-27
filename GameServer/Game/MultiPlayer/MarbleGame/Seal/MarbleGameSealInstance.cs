using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;

public class MarbleGameSealInstance(int itemId, int sealId)
{
    public int Id { get; set; } = itemId;
    public int SealId { get; set; } = sealId;
    public int CurHp { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public float Mass { get; set; }
    public float MaxSpeed { get; set; }
    public float Size { get; set; }
    public bool OnStage { get; set; } = true;
    public MarbleSealVector Position { get; set; } = new();
    public MarbleSealVector Rotation { get; set; } = new();
    public MarbleSealVector Velocity { get; set; } = new();

    public MarbleGameSealInstance Clone()
    {
        return new MarbleGameSealInstance(Id, SealId)
        {
            CurHp = CurHp,
            MaxHp = MaxHp,
            Attack = Attack,
            Size = Size,
            Mass = Mass,
            MaxSpeed = MaxSpeed,
            OnStage = OnStage,
            Position = Position.Clone(),
            Rotation = Rotation.Clone(),
            Velocity = Velocity.Clone()
        };
    }
}