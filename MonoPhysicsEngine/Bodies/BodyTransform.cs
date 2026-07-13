using System;

namespace MonoPhysicsEngine;

internal readonly struct BodyTransform
{
    public readonly MonoVector Position;
    public readonly float Sin;
    public readonly float Cos;
    
    public readonly static BodyTransform Zero = new BodyTransform(0f, 0f, 0f);

    public BodyTransform(MonoVector position, float angle)
    {
        Position = new MonoVector(position.X, position.Y);
        Sin = MathF.Sin(angle);
        Cos = MathF.Cos(angle);
    }
    public BodyTransform(float x, float y, float angle)
    {
        Position = new  MonoVector(x, y);
        Sin = MathF.Sin(angle);
        Cos = MathF.Cos(angle);
    }
}