using System;
using MonoPhysicsEngine.Content;

namespace MonoPhysicsEngine;

public static class Collisions
{
    public static bool CheckGeneralCollision(RigidBody bodyA, RigidBody bodyB, out MonoVector normal, out float depth)
    {
        ShapeType shapeA = bodyA.ShapeType;
        ShapeType shapeB = bodyB.ShapeType;
                
        normal = MonoVector.Zero;
        depth = 0f;

        if (shapeA == shapeB)
        {
            if (shapeA == ShapeType.Circle)
            {
                return CheckCircleCollision(bodyA.Position, bodyB.Position, bodyA.Radius, bodyB.Radius, out normal, out depth);
            }
            if (shapeA == ShapeType.Box)
            {
                return CheckPolygonCollision(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out normal, out depth);
            }
        }
        else
        {
            if (shapeA == ShapeType.Circle && shapeB == ShapeType.Box)
            {
                return CheckCirclePolygonCollision(bodyA.Position, bodyA.Radius, bodyB.GetTransformedVertices(), out normal, out depth);
            }
            if (shapeA == ShapeType.Box && shapeB == ShapeType.Circle)
            {
                bool result = CheckCirclePolygonCollision(bodyB.Position, bodyB.Radius, bodyA.GetTransformedVertices(), out normal, out depth);
                normal = -normal;
                return result;
            }
        }

        return false;
    }
    
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

    public static bool CheckPolygonCollision(MonoVector[] verticesA, MonoVector[] verticesB, out MonoVector normal, out float depth)
    {
        normal = MonoVector.Zero;
        depth = float.MaxValue;

        int totalEdges = verticesA.Length + verticesB.Length;
        for (int i = 0; i < totalEdges; i++)
        {
            MonoVector[] currentShape = i < verticesA.Length ? verticesA : verticesB;
            int index = i < verticesA.Length ? i : i - verticesA.Length;
            
            MonoVector va1 = currentShape[index];
            MonoVector va2 = currentShape[(index + 1) % currentShape.Length];

            MonoVector edge = va2 - va1;
            MonoVector axis = new MonoVector(-edge.Y, edge.X).Normalize();
            
            Util.ProjectVerticesOnAxis(verticesA, axis, out float minA, out float maxA);
            Util.ProjectVerticesOnAxis(verticesB, axis, out float minB, out float maxB);
            
            if (minA >= maxB || minB >= maxA) return false;

            float axisDepth = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }
        }

        MonoVector centerA = Util.GetPolygonCenter(verticesA);
        MonoVector centerB = Util.GetPolygonCenter(verticesB);
        if (MonoVector.Dot(normal, centerA - centerB) < 0) { normal = -normal; }
        
        return true;
    }

    public static bool CheckCirclePolygonCollision(MonoVector centerA, float radiusA, MonoVector[] verticesB, out MonoVector normal, out float depth)
    {
        normal = MonoVector.Zero;
        depth = float.MaxValue;

        MonoVector closestVertex = verticesB[0];
        float minDistance = (closestVertex - centerA).LengthSquared;
        float distance;

        for (int i = 0; i < verticesB.Length; i++)
        {
            MonoVector va1 = verticesB[i];
            MonoVector va2 = verticesB[(i + 1) % verticesB.Length];

            distance = (va1 - centerA).LengthSquared;
            if (distance < minDistance)
            {
                closestVertex = va1;
                minDistance = distance;
            }

            MonoVector edge = va2 - va1;
            MonoVector axis = new MonoVector(-edge.Y, edge.X).Normalize();
            
            Util.ProjectCircleOnAxis(centerA, radiusA, axis, out float minA, out float maxA);
            Util.ProjectVerticesOnAxis(verticesB, axis, out float minB, out float maxB);
            
            if (minA >= maxB || minB >= maxA) return false;

            float axisDepth = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }
        }

        MonoVector cornerAxis = centerA - closestVertex;
        if (cornerAxis.LengthSquared > 0.01f)
        {
            cornerAxis = cornerAxis.Normalize();

            Util.ProjectCircleOnAxis(centerA, radiusA, cornerAxis, out float minA, out float maxA);
            Util.ProjectVerticesOnAxis(verticesB, cornerAxis, out float minB, out float maxB);

            if (minA >= maxB || minB >= maxA) return false;

            float axisDepth = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = cornerAxis;
            }
        }

        MonoVector centerB = Util.GetPolygonCenter(verticesB);
        if (MonoVector.Dot(normal, centerA - centerB) < 0) { normal = -normal; }

        return true;
    }

    public static void ResolveCollision(RigidBody bodyA, RigidBody bodyB, MonoVector normal, float depth)
    {
        MonoVector relativeVelocity = bodyA.LinearVelocity - bodyB.LinearVelocity;

        if (MonoVector.Dot(relativeVelocity, normal) > 0f) return; // Bodies are already moving apart
        
        float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);
        float j = -(1f + e) * MonoVector.Dot(relativeVelocity, normal);
        j /= bodyA.InvMass + bodyB.InvMass;

        MonoVector impulse = j * normal;
        
        bodyA.LinearVelocity += impulse * bodyA.InvMass;
        bodyB.LinearVelocity -= impulse * bodyB.InvMass;
    }
}