using System;
using System.Collections.Generic;
using MonoPhysicsEngine.Content;
using Microsoft.Xna.Framework;
using MonoUtils.Graphics;

namespace MonoPhysicsEngine;

public sealed class World
{
    public static readonly float MinBodySize = 1f * 1f; // cm2
    public static readonly float MaxBodySize = 64f * 64f;

    public static readonly float MinDensity = 0.5f; // g/cm3
    public static readonly float MaxDensity = 25f;

    public static readonly int MinIterations = 1;
    public static readonly int MaxIterations = 128;

    public static readonly float SCREEN_SPAWN_PADDING = 20f;

    public int BodyCount => bodies.Count;

    private MonoVector gravity;
    
    private List<RigidBody> bodies;
    private Vector2[] vertexBuffer;
    private List<MonoManifold> contacts;

    public World()
    {
        // gravity = new MonoVector(0, -981f);
        gravity = new MonoVector(0, -600f);

        bodies = new List<RigidBody>();
        contacts = new List<MonoManifold>();
    }

    public void AddBody(RigidBody body)
    {
        bodies.Add(body);
    }

    public bool RemoveBody(RigidBody body)
    {
        return bodies.Remove(body);
    }

    public RigidBody GetBody(int index)
    {
        if (index >= 0 && index < bodies.Count) return bodies[index];
        throw new IndexOutOfRangeException("Body index out of range.");
    }

    public void Step(float deltaTime, int iterations = 24)
    {
        iterations = Math.Clamp(iterations, MinIterations, MaxIterations);
        float subStepDt = deltaTime / iterations;

        for (int t = 0; t < iterations; t++)
        {
            /* --- Movement --- */
            for (int i = 0; i < bodies.Count; i++)
            {
                if (!bodies[i].IsStatic) { bodies[i].AddForce(gravity * bodies[i].Mass); }
                bodies[i].Step(subStepDt);
            }
            
            /* --- Collision Detection --- */
            
            for (int i = 0; i < bodies.Count; i++)
            {
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    RigidBody bodyA = bodies[i];
                    RigidBody bodyB = bodies[j];

                    if (bodyA.IsStatic && bodyB.IsStatic) continue;

                    if (Collisions.CheckGeneralCollision(bodyA, bodyB, out MonoVector normal, out float depth))
                    {
                        float totalInvMass = bodyA.InvMass + bodyB.InvMass;
                        if (totalInvMass > 0f)
                        {
                            // const float percent = 0.4f;
                            // const float slop = 0.03f;
                            
                            // float correctionMagnitude = Math.Max(depth - slop, 0.0f) / totalInvMass * percent;
                            float correctionMagnitude = depth / totalInvMass;
                            MonoVector correctionVector = normal * correctionMagnitude;
                            
                            if (!bodyA.IsStatic) bodyA.MoveBy(correctionVector * bodyA.InvMass);
                            if (!bodyB.IsStatic) bodyB.MoveBy(-correctionVector * bodyB.InvMass);
                        }

                        MonoManifold contact = new MonoManifold(bodyA, bodyB, normal, depth, MonoVector.Zero, MonoVector.Zero, 0);
                        contacts.Add(contact);
                        
                        // Collisions.ResolveCollision(bodyA, bodyB, normal, depth);
                    }
                }
            }

            /* --- Collision Resolution --- */
            for (int i = contacts.Count - 1; i >= 0; i--)
            {
                MonoManifold contact = contacts[i];
                Collisions.ResolveCollision(in contact);
                contacts.RemoveAt(i);
            }
        }
    }

    public void DrawShapes(Shapes shapes, World world, bool renderAABBs)
    {
        for (int i = 0; i < world.BodyCount; i++)
        {
            RigidBody body = bodies[i];
            ShapeType type = body.ShapeType;
            bool fill = body.FillMode == Shapes.FillMode.Filled;
            
            if (type == ShapeType.Circle)
            {
                if (fill) shapes.DrawCircle(body.Position.ToVector2(), body.Radius, body.FillColor, Shapes.FillMode.Filled);
                shapes.DrawCircle(body.Position.ToVector2(), body.Radius, body.BorderColor, Shapes.FillMode.Border);
            }
            else if (type == ShapeType.Box)
            {
                Util.ToVector2Array(body.GetTransformedVertices(), ref vertexBuffer);
                if (fill) shapes.DrawPolygon(vertexBuffer, body.Triangles, body.FillColor, Shapes.FillMode.Filled);
                shapes.DrawPolygon(vertexBuffer, body.Triangles, body.BorderColor, Shapes.FillMode.Border);
            }
            if (renderAABBs)
            {
                MonoAABB box = body.GetAABB();
                Vector2 center = ((box.Min + box.Max) / 2f).ToVector2();
                shapes.DrawRectangle(center, box.Max.X - box.Min.X, box.Max.Y - box.Min.Y, Color.White, Shapes.FillMode.Border);
            }
        }
    }
}