using System;
using Microsoft.Xna.Framework;
using MonoPhysicsEngine.Content;
using MonoUtils.Graphics;

namespace MonoPhysicsEngine;

public static class Util
{
    private static Random rng = new Random();
    
    public static void ToVector2Array(MonoVector[] src, ref Vector2[] dst)
    {
        if (dst is null || src.Length != dst.Length)
        {
            dst = new Vector2[src.Length];
        }

        for (int i = 0; i < src.Length; i++)
        {
            dst[i] = src[i].ToVector2();
        }
    }

    public static void ProjectVerticesOnAxis(MonoVector[] vertices, MonoVector axis, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            MonoVector vertex = vertices[i];
            float proj = MonoVector.Dot(vertex, axis);
            
            if (proj < min) min = proj;
            if (proj > max) max = proj;
        }
    }

    public static void ProjectCircleOnAxis(MonoVector center, float radius, MonoVector axis, out float min, out float max)
    {
        MonoVector axisParallelRadius = axis.Normalize() * radius;

        MonoVector pointA = center - axisParallelRadius;
        MonoVector pointB = center + axisParallelRadius;
        
        min = MonoVector.Dot(pointA, axis);
        max = MonoVector.Dot(pointB, axis);

        if (min > max)
        {
            (min, max) = (max, min);
        }
    }
    
    public static MonoVector GetPolygonCenter(MonoVector[] vertices)
    {
        MonoVector total = MonoVector.Zero;
        for (int i = 0; i < vertices.Length; i++)
        {
            total += vertices[i];
        }
        return total / vertices.Length;
    }

    public static void SpawnRandomBodies(World world, int count, Camera camera, Color default_border_color, float screenPadding = 20f, ShapeType? shapeType = null, float? area = null, float? density = null, Shapes.FillMode fillMode = Shapes.FillMode.Filled)
    {
        Random rng = new Random();
        
        camera.GetScreenBounds(out float left, out float right, out float bottom, out float top);
        
        for (int i = 0; i < count; i++)
        {
            RigidBody body = null;
            string msg = null;
            bool success = false;
            
            int _shapeType = (shapeType is null) ? rng.Next(0, Enum.GetNames(typeof(ShapeType)).Length) : (int)shapeType;
            float _area = (area is null) ? GetBiasedRandom(World.MinBodySize, World.MaxBodySize, 100f, 0.5f) : (float)area;
            float _density = (density is null) ? GetBiasedRandom(World.MinDensity, World.MaxDensity, 3f, 0.75f) : (float)density;
            
            int x = rng.Next((int)(left + screenPadding), (int)(right - screenPadding));
            int y = rng.Next((int)(bottom + screenPadding), (int)(top - screenPadding));
            
            Color color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            
            if (_shapeType == 0)
            {
                float radius = MathF.Sqrt(_area / MathF.PI);
                success = RigidBody.CreateCircleBody(new MonoVector(x, y), radius, _density, false, 1f, fillMode, color, default_border_color, out body, out msg);
            }
            else if (_shapeType == 1)
            {
                float maxDimensions = MathF.Sqrt(_area);
                float width = maxDimensions * (float)(rng.NextDouble() * 1.0 + 0.5);
                float height = _area / width;
        
                success = RigidBody.CreateBoxBody(new MonoVector(x, y), width, height, _density, false, 1f, fillMode, color, default_border_color, out body, out msg);
            }
            
            if (success) { world.AddBody(body); }
            else Console.WriteLine($"[{i}] {msg}");
        }
    }

    public static void WrapScreen(Camera camera, World world)
    {
        camera.GetScreenBounds(out float left, out float right, out float bottom, out float top);
        
        for (int i = 0; i < world.BodyCount; i++)
        {
            RigidBody body = world.GetBody(i);
            
            if (body.Position.X < left) body.MoveTo(new MonoVector(right, body.Position.Y));
            if (body.Position.X > right) body.MoveTo(new MonoVector(left, body.Position.Y));
            if (body.Position.Y < bottom) body.MoveTo(new MonoVector(body.Position.X, top));
            if (body.Position.Y > top) body.MoveTo(new MonoVector(body.Position.X, bottom));
        }
    }

    public static float GetBiasedRandom(float min, float max, float peak, float peakBias = 0.7f)
    {
        peak = Math.Clamp(peak, min, max);
        peakBias = Math.Clamp(peakBias, 0.0f, 1.0f);
        
        float r1 = (float)rng.NextDouble();
        float r2 = (float)rng.NextDouble();
        
        float upwardSlopeBias = 1.0f - (r2 * r2); 
        float downwardTailBias = r2 * r2;
        
        if (r1 < peakBias) { return min + upwardSlopeBias * (peak - min); }
        return peak + downwardTailBias * (max - peak);
    }
}