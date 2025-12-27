using System.Numerics;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Physics;

public class CollisionSimulator(
    float leftBound,
    float rightBound,
    float topBound,
    float bottomBound,
    float deceleration = 15f)
{
    public const float StepTime = 0.001f;
    public float Deceleration = deceleration;
    public List<Ball> Balls { get; } = [];
    public int LaunchTeam { get; set; } = 0;
    public List<object> Records { get; } = [];
    public float CurTime { get; set; }
    private float LeftBound { get; } = leftBound;
    private float RightBound { get; } = rightBound;
    private float TopBound { get; } = topBound;
    private float BottomBound { get; } = bottomBound;

    public void AddBall(int id, Vector2 position, float mass, float radius, Vector2? velocity = null,
        bool isStatic = false, int hp = 100, int atk = 0)
    {
        Balls.Add(new Ball(id, position, mass, radius, velocity, isStatic, hp, atk));
    }

    public void AdvanceTime(float time)
    {
        CurTime += time;

        foreach (var ball in Balls.Where(ball => !ball.IsStatic))
        {
            if (ball.Velocity.Length() < 0.01f)
            {
                if (ball.Velocity != Vector2.Zero)
                {
                    ball.Velocity = Vector2.Zero;
                    Records.Add(new StopRecord(CurTime, ball.GetSnapshot()));
                }

                continue;
            }

            ball.Position += ball.Velocity * time;
            ball.Velocity -= Vector2.Normalize(ball.Velocity) * Deceleration * time;
        }

        CheckWallCollision();
        CheckBallCollision();
    }

    public void CheckWallCollision()
    {
        foreach (var ball in Balls.Where(ball => !ball.IsStatic))
        {
            if (ball.Velocity.Length() < 0.001f) continue;

            var collide = false;
            var collisionPosition = Vector2.Zero;
            if (ball.Position.X + ball.Radius >= RightBound || ball.Position.X - ball.Radius <= LeftBound)
            {
                collisionPosition = ball.Position with { X = MathF.Sign(ball.Velocity.X) * RightBound };
                ball.Velocity = ball.Velocity with { X = -ball.Velocity.X };

                collide = true;
            }

            if (ball.Position.Y + ball.Radius >= TopBound || ball.Position.Y - ball.Radius <= BottomBound)
            {
                collisionPosition = ball.Position with { Y = MathF.Sign(ball.Velocity.Y) * TopBound };
                ball.Velocity = ball.Velocity with { Y = -ball.Velocity.Y };

                collide = true;
            }

            if (collide)
            {
                ball.StageInitialVelocity = ball.Velocity;
                Records.Add(new CollisionRecord(CurTime, ball.GetSnapshot(), null, collisionPosition));
            }
        }
    }

    public void CheckBallCollision()
    {
        for (var i = 0; i < Balls.Count; i++)
        for (var j = i + 1; j < Balls.Count; j++)
        {
            var ballA = Balls[i];
            var ballB = Balls[j];
            if (ballA.IsStatic || ballB.IsStatic) continue; // skip static balls

            var distance = Vector2.Distance(ballA.Position, ballB.Position);
            if (distance <= ballA.Radius + ballB.Radius) HandleBallCollision(ballA, ballB);
        }
    }

    public void HandleBallCollision(Ball ballA, Ball ballB)
    {
        var deltaPos = ballB.Position - ballA.Position;
        if (deltaPos == Vector2.Zero) return;

        // get normal and tangent vectors
        var normal = Vector2.Normalize(deltaPos);
        var tangent = new Vector2(-normal.Y, normal.X);

        // split velocity into normal and tangent components
        var v1n = Vector2.Dot(ballA.Velocity, normal);
        var v1t = Vector2.Dot(ballA.Velocity, tangent);
        var v2n = Vector2.Dot(ballB.Velocity, normal);
        var v2t = Vector2.Dot(ballB.Velocity, tangent);

        var massA = ballA.Mass;
        var massB = ballB.Mass;
        var totalMass = massA + massB;

        // switch to normal velocity
        var newV1n = (v1n * (massA - massB) + 2 * massB * v2n) / totalMass;
        var newV2n = (v2n * (massB - massA) + 2 * massA * v1n) / totalMass;

        // combine velocity
        ballA.Velocity = newV1n * normal + v1t * tangent;
        ballB.Velocity = newV2n * normal + v2t * tangent;

        // fix pos
        var overlap = ballA.Radius + ballB.Radius - deltaPos.Length();
        if (overlap > 0)
        {
            var correction = normal * overlap;
            if (!ballA.IsStatic) ballA.Position -= correction * massB / totalMass;
            if (!ballB.IsStatic) ballB.Position += correction * massA / totalMass;
        }

        ballA.StageInitialVelocity = ballA.Velocity;
        ballB.StageInitialVelocity = ballB.Velocity;

        // record
        var collisionPos = ballA.Position + normal * ballA.Radius;
        Records.Add(new CollisionRecord(CurTime, ballA.GetSnapshot(), ballB.GetSnapshot(), collisionPos));

        // check if ball is dead
        if (ballA.Id / 100 != ballB.Id / 100)
        {
            // different teams
            if (ballA.Id / 100 == LaunchTeam)
            {
                ballB.Hp -= ballA.Atk;
                if (ballB.Hp <= 0)
                {
                    ballB.Velocity = Vector2.Zero;
                    ballB.StageInitialVelocity = Vector2.Zero;
                    ballB.IsStatic = true;
                }
            }
            else
            {
                ballA.Hp -= ballB.Atk;
                if (ballA.Hp <= 0)
                {
                    ballA.Velocity = Vector2.Zero;
                    ballA.StageInitialVelocity = Vector2.Zero;
                    ballA.IsStatic = true;
                }
            }
        }
    }

    public void Simulate()
    {
        while (!AllObjectStopped())
        {
            AdvanceTime(StepTime);
            foreach (var ball in Balls.Where(x =>
                         x.StageInitialVelocity != Vector2.Zero && x.Velocity.Length() > 0.01f))
            {
                var speed = ball.Velocity.Length();
                if (ball.StageInitialVelocity.Length() / 2 > speed)
                {
                    ball.StageInitialVelocity = Vector2.Zero; // avoid infinite loop
                    Records.Add(new ChangeSpeedRecord(CurTime, ball.GetSnapshot()));
                }
            }
        }
    }

    public bool AllObjectStopped()
    {
        return Balls.All(ball => ball.Velocity.Length() == 0);
    }
}