using MonoPhysicsEngine.Content;

namespace MonoPhysicsEngine;

public readonly struct MonoManifold
{
    public readonly RigidBody BodyA;
    public readonly RigidBody BodyB;
    public readonly MonoVector Normal;
    public readonly float Depth;
    public readonly MonoVector Contact1;
    public readonly MonoVector Contact2;
    public readonly int ContactCount;

    public MonoManifold(RigidBody bodyA, RigidBody bodyB, MonoVector normal, float depth, MonoVector contact1, MonoVector contact2, int contactCount)
    {
        BodyA = bodyA;
        BodyB = bodyB;
        Normal = normal;
        Depth = depth;
        Contact1 = contact1;
        Contact2 = contact2;
        ContactCount = contactCount;
    }
}