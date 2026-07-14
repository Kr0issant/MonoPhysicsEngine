using Microsoft.Xna.Framework;

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
}