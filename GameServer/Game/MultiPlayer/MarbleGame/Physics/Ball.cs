using System.Numerics;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Physics;

public class Ball(
    int id,
    Vector2 position,
    float mass,
    float radius,
    Vector2? velocity = null,
    bool isStatic = false,
    int hp = 100,
    int atk = 0)
{
    public int Id { get; } = id;
    public Vector2 Position { get; set; } = position;
    public Vector2 Velocity { get; set; } = velocity ?? Vector2.Zero;
    public Vector2 StageInitialVelocity { get; set; } = velocity ?? Vector2.Zero;
    public float Radius { get; } = radius;
    public float Mass { get; } = mass;
    public bool IsStatic { get; set; } = isStatic;
    public int Hp { get; set; } = hp;
    public int Atk { get; set; } = atk;

    public BallSnapshot GetSnapshot()
    {
        return new BallSnapshot
        {
            Id = Id,
            Position = Position,
            Velocity = Velocity
        };
    }
}