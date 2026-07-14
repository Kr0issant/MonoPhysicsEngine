using System;
using Microsoft.Xna.Framework;
using MonoPhysicsEngine.Content;
using MonoUtils.Graphics;

namespace MonoPhysicsEngine;

public static class Util
{
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
            float _area = (area is null) ? (float)(rng.NextDouble() * (World.MaxBodySize - World.MinBodySize) + World.MinBodySize) : (float)area;
            float _density = (density is null) ? (float)(rng.NextDouble() * (World.MaxDensity - World.MinDensity) + World.MinDensity) : (float)density;
            
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
                float maxDimensions = MathF.Sqrt((float)area);
                float width = maxDimensions * (float)(rng.NextDouble() * 1.0 + 0.5);
                float height = (float)area / width;
        
                success = RigidBody.CreateBoxBody(new MonoVector(x, y), width, height, _density, false, 1f, fillMode, color, default_border_color, out body, out msg);
            }
            
            if (success) { world.AddBody(body); }
            else Console.WriteLine($"[{i}] {msg}");
        }
    }
}