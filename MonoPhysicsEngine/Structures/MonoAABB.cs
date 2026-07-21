namespace MonoPhysicsEngine;

public readonly struct MonoAABB
{
    public readonly MonoVector Min;
    public readonly MonoVector Max;

    public MonoAABB(MonoVector min, MonoVector max)
    {
        Min = min;
        Max = max;
    }

    public MonoAABB(float minX, float minY, float maxX, float maxY)
    {
        Min = new MonoVector(minX, minY);
        Max = new MonoVector(maxX, maxY);
    }
}