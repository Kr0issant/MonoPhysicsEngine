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
}