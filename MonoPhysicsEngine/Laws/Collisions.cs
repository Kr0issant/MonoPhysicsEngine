using System;
using MonoPhysicsEngine.Content;

namespace MonoPhysicsEngine;

public static class Collisions
{
    /* --- Detection --- */
    public static bool CheckCollision(RigidBody bodyA, RigidBody bodyB, out MonoVector normal, out float depth)
    {
        ShapeType shapeA = bodyA.ShapeType;
        ShapeType shapeB = bodyB.ShapeType;
                
        normal = MonoVector.Zero;
        depth = 0f;

        if (shapeA == shapeB)
        {
            if (shapeA == ShapeType.Circle)
            {
                return CheckCircleCollision(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius, out normal, out depth);
            }
            if (shapeA == ShapeType.Box)
            {
                return CheckPolygonCollision(bodyA.Position, bodyA.GetTransformedVertices(), bodyB.Position, bodyB.GetTransformedVertices(), out normal, out depth);
            }
        }
        else
        {
            if (shapeA == ShapeType.Circle && shapeB == ShapeType.Box)
            {
                return CheckCirclePolygonCollision(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertices(), out normal, out depth);
            }
            if (shapeA == ShapeType.Box && shapeB == ShapeType.Circle)
            {
                bool result = CheckCirclePolygonCollision(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertices(), out normal, out depth);
                normal = -normal;
                return result;
            }
        }

        return false;
    }

    public static bool CheckAABBCollision(MonoAABB a, MonoAABB b)
    {
        if (a.Max.X <= b.Min.X || b.Max.X <= a.Min.X) return false;
        if (a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y) return false;

        return true;
    }

    private static bool CheckCircleCollision(MonoVector centerA, float radiusA, MonoVector centerB, float radiusB, out MonoVector normal, out float depth)
    {
        normal = MonoVector.Zero;
        depth = 0f;

        float distance = MonoVector.DistanceBetween(centerA, centerB);

        if (distance >= radiusA + radiusB) return false;
        
        normal = distance == 0 ? new MonoVector(-1, 0) : (centerA - centerB).Normalize();
        depth = (radiusA + radiusB) - distance;
        return true;
    }

    private static bool CheckPolygonCollision(MonoVector centerA, MonoVector[] verticesA, MonoVector centerB, MonoVector[] verticesB, out MonoVector normal, out float depth)
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
        
        if (MonoVector.Dot(normal, centerA - centerB) < 0) { normal = -normal; }
        
        return true;
    }

    private static bool CheckCirclePolygonCollision(MonoVector circleCenter, float circleRadius, MonoVector polygonCenter, MonoVector[] polygonVertices, out MonoVector normal, out float depth)
    {
        normal = MonoVector.Zero;
        depth = float.MaxValue;

        MonoVector closestVertex = polygonVertices[0];
        float minDistance = (closestVertex - circleCenter).LengthSquared;
        float distance;

        for (int i = 0; i < polygonVertices.Length; i++)
        {
            MonoVector va1 = polygonVertices[i];
            MonoVector va2 = polygonVertices[(i + 1) % polygonVertices.Length];

            distance = (va1 - circleCenter).LengthSquared;
            if (distance < minDistance)
            {
                closestVertex = va1;
                minDistance = distance;
            }

            MonoVector edge = va2 - va1;
            MonoVector axis = new MonoVector(-edge.Y, edge.X).Normalize();
            
            Util.ProjectCircleOnAxis(circleCenter, circleRadius, axis, out float minA, out float maxA);
            Util.ProjectVerticesOnAxis(polygonVertices, axis, out float minB, out float maxB);
            
            if (minA >= maxB || minB >= maxA) return false;

            float axisDepth = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }
        }

        MonoVector cornerAxis = circleCenter - closestVertex;
        if (cornerAxis.LengthSquared > 0.01f)
        {
            cornerAxis = cornerAxis.Normalize();

            Util.ProjectCircleOnAxis(circleCenter, circleRadius, cornerAxis, out float minA, out float maxA);
            Util.ProjectVerticesOnAxis(polygonVertices, cornerAxis, out float minB, out float maxB);

            if (minA >= maxB || minB >= maxA) return false;

            float axisDepth = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = cornerAxis;
            }
        }

        if (MonoVector.Dot(normal, circleCenter - polygonCenter) < 0) { normal = -normal; }

        return true;
    }
    
    /* --- Resolution --- */
    public static void ResolveCollision(in MonoManifold contact)
    {
        RigidBody bodyA = contact.BodyA;
        RigidBody bodyB = contact.BodyB;
        MonoVector normal = contact.Normal;
        float depth = contact.Depth;
        
        MonoVector relativeVelocity = bodyA.LinearVelocity - bodyB.LinearVelocity;

        if (MonoVector.Dot(relativeVelocity, normal) > 0f) return; // Bodies are already moving apart
        
        float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);
        float j = -(1f + e) * MonoVector.Dot(relativeVelocity, normal);
        j /= bodyA.InvMass + bodyB.InvMass;

        MonoVector impulse = j * normal;
        
        bodyA.LinearVelocity += impulse * bodyA.InvMass;
        bodyB.LinearVelocity -= impulse * bodyB.InvMass;
    }
    
    /* --- Contact Points --- */
    public static void GetContactPoints(RigidBody bodyA, RigidBody bodyB, out MonoVector contactPoint1, out MonoVector contactPoint2, out int contactCount)
    {
        ShapeType shapeA = bodyA.ShapeType;
        ShapeType shapeB = bodyB.ShapeType;

        contactPoint1 = MonoVector.Zero;
        contactPoint2 = MonoVector.Zero;
        contactCount = 0;
        
        if (shapeA == shapeB)
        {
            if (shapeA == ShapeType.Circle)
            {
                GetCircleContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contactPoint1);
                contactCount = 1;
            }

            if (shapeA == ShapeType.Box)
            {
                GetPolygonContactPoint(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out contactPoint1, out contactPoint2,  out contactCount);
            }
        }
        else
        {
            if (shapeA == ShapeType.Circle && shapeB == ShapeType.Box) 
            {
                GetCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertices(), out contactPoint1);
                contactCount = 1;
            }
            if (shapeA == ShapeType.Box && shapeB == ShapeType.Circle) 
            {
                GetCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertices(), out contactPoint1);
                contactCount = 1;
            }
        }
    }
    
    private static void GetCircleContactPoint(MonoVector centerA, float radiusA, MonoVector centerB, out MonoVector contactPoint)
    {
        MonoVector dir = (centerB - centerA).Normalize();
        
        contactPoint = centerA + radiusA * dir;
    }

    private static void GetCirclePolygonContactPoint(MonoVector circleCenter, float circleRadius, MonoVector polygonCenter, MonoVector[] polygonVertices, out MonoVector contactPoint)
    {
        contactPoint = MonoVector.Zero;
        float minDistanceSquared = float.MaxValue;
        
        for (int i = 0; i < polygonVertices.Length; i++)
        {
            MonoVector va = polygonVertices[i];
            MonoVector vb = polygonVertices[(i + 1) % polygonVertices.Length];
            
            Util.GetPointSegmentDistance(circleCenter, va, vb, out float distanceSquared, out MonoVector cp);
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared =  distanceSquared;
                contactPoint = cp;
            }
        }
    }

    private static void GetPolygonContactPoint(MonoVector[] verticesA, MonoVector[] verticesB, out MonoVector contactPoint1, out MonoVector contactPoint2, out int contactCount)
    {
        contactPoint1 = MonoVector.Zero;
        contactPoint2 = MonoVector.Zero;
        contactCount = 0;
        
        float minDistanceSquared = float.MaxValue;
        
        void TestPointSegment(MonoVector p, MonoVector va, MonoVector vb, ref MonoVector contactPoint1, ref MonoVector contactPoint2, ref int contactCount)
        {
            Util.GetPointSegmentDistance(p, va, vb, out float distanceSquared, out MonoVector cp);

            if (distanceSquared < minDistanceSquared - 0.05f)
            {
                minDistanceSquared = distanceSquared;
                contactPoint1 = cp;
                contactPoint2 = MonoVector.Zero;
                contactCount = 1;
            }
            
            else if (Util.IsNearlyEqual(distanceSquared, minDistanceSquared))
            {
                if (contactCount == 1)
                {
                    if (!Util.IsNearlyEqual(cp, contactPoint1))
                    {
                        contactPoint2 = cp;
                        contactCount = 2;
                    }
                }
            }
        }

        for (int i = 0; i < verticesA.Length; i++)
        {
            for (int j = 0; j < verticesB.Length; j++)
            {
                TestPointSegment(verticesA[i], verticesB[j], verticesB[(j + 1) % verticesB.Length], ref contactPoint1, ref contactPoint2, ref contactCount);
            }
        }
        for (int i = 0; i < verticesB.Length; i++)
        {
            for (int j = 0; j < verticesA.Length; j++)
            {
                TestPointSegment(verticesB[i], verticesA[j], verticesA[(j + 1) % verticesA.Length], ref contactPoint1, ref contactPoint2, ref contactCount);
            }
        }
    }
}