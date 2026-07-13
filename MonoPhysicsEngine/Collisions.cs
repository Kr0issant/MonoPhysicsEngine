namespace MonoPhysicsEngine;

public static class Collisions
{
    public static bool CheckCircleCollision(MonoVector centerA, MonoVector centerB, float radiusA, float radiusB, out MonoVector normal, out float depth)
    {
        normal = MonoVector.Zero;
        depth = 0f;

        float distance = MonoVector.DistanceBetween(centerA, centerB);

        if (distance >= radiusA + radiusB) return false;
        
        normal = distance == 0 ? new MonoVector(-1, 0) : (centerA - centerB).Normalize();
        depth = (radiusA + radiusB) - distance;
        return true;
    }
}