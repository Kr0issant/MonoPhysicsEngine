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

    public static readonly float SCREEN_SPAWN_PADDING = 20f;

    public int BodyCount => bodies.Count;

    private MonoVector gravity;
    
    private List<RigidBody> bodies;
    private Vector2[] vertexBuffer;

    public World()
    {
        gravity = new MonoVector(0, -9.81f);

        bodies = new List<RigidBody>();
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

    public void Step(float deltaTime)
    {
        /* --- Movement --- */
        for (int i = 0; i < bodies.Count; i++)
        {
            // bodies[i].AddForce(gravity * bodies[i].Mass);
            bodies[i].Step(deltaTime);
        }
        
        /* --- Collision Detection and Resolution --- */
        
        for (int i = 0; i < bodies.Count; i++)
        {
            RigidBody bodyA = bodies[i];
            for (int j = i + 1; j < bodies.Count; j++)
            {
                RigidBody bodyB = bodies[j];

                if (Collisions.CheckGeneralCollision(bodyA, bodyB, out MonoVector normal, out float depth))
                {
                    bodyA.MoveBy(normal * depth * (bodyB.Mass / (bodyA.Mass + bodyB.Mass)));
                    bodyB.MoveBy(-normal * depth * (bodyA.Mass / (bodyA.Mass + bodyB.Mass)));
                    Collisions.ResolveCollision(bodyA, bodyB, normal, depth);
                }
            }
        }
    }

    public void DrawShapes(Shapes shapes, World world)
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
        }
    }
}