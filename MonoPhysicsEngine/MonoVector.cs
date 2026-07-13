using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace MonoPhysicsEngine;

public readonly struct MonoVector : IEquatable<MonoVector>
{
    public readonly float X;
    public readonly float Y;
    public float Length => MathF.Sqrt((X * X) + (Y * Y));
    public float LengthSquared => (X * X) + (Y * Y);

    public static readonly MonoVector Zero = new MonoVector(0f, 0f);

    public MonoVector(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector FromVector2(Vector2 v)
    {
        return new MonoVector(v.X, v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector operator +(MonoVector a, MonoVector b)
    {
        return new MonoVector(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector operator -(MonoVector a, MonoVector b)
    {
        return new MonoVector(a.X - b.X, a.Y - b.Y);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector operator -(MonoVector v)
    {
        return new MonoVector(-v.X, -v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector operator *(MonoVector v, float s)
    {
        return new MonoVector(v.X * s, v.Y * s);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator *(MonoVector a, MonoVector b)
    {
        return Dot(a, b);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoVector operator /(MonoVector v, float s)
    {
        return new MonoVector(v.X / s, v.Y / s);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(MonoVector a, MonoVector b)
    {
        return (a.X * b.X) + (a.Y * b.Y);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross(MonoVector a, MonoVector b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MonoVector Normalize()
    {
        return this / Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(MonoVector a, MonoVector b)
    {
        return a.Equals(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(MonoVector a, MonoVector b)
    {
        return !a.Equals(b);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(MonoVector other)
    {
        return (this.X == other.X) && (this.Y == other.Y);
    }

    public override bool Equals(object obj)
    {
        if (obj is MonoVector other)
        {
            return this.Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({this.X}, {this.Y})";
    }

    public static float DistanceBetween(MonoVector a, MonoVector b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    public static float AngleBetween(MonoVector a, MonoVector b)
    {
        return (float)Math.Acos(Dot(a, b));
    }
}