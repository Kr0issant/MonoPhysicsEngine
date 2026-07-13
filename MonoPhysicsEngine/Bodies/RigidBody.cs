using System;

namespace MonoPhysicsEngine.Content;

public enum ShapeType
{
    Circle = 0,
    Box = 1,
}

public sealed class RigidBody
{
    public readonly float Density;
    public readonly float Mass;
    public readonly float Restitution;
    public readonly float Area;
    
    public readonly bool IsStatic;
    public readonly ShapeType ShapeType;
    
    public readonly float Radius;
    public readonly float Width;
    public readonly float Height;
    
    private MonoVector position;
    private MonoVector linearVelocity;
    private float angularVelocity;
    private float rotation;

    private MonoVector[] vertices;
    private MonoVector[] transformedVertices;
    private bool transformUpdateRequired;
    public int[] Triangles;
    
    public MonoVector Position => position;
    public MonoVector LinearVelocity => linearVelocity;
    public float AngularVelocity => angularVelocity;
    public float Rotation => rotation;

    private RigidBody(MonoVector position, float density, float mass, float restitution, float area, bool isStatic, ShapeType shapeType, float radius, float width, float height)
    {
        this.Density = density;
        this.Mass = mass;
        this.Restitution = restitution;
        this.Area = area;
        
        this.IsStatic = isStatic;
        this.ShapeType = shapeType;
        
        this.Radius = radius;
        this.Width = width;
        this.Height = height;
        
        this.position = position;
        this.linearVelocity = MonoVector.Zero;
        this.angularVelocity = 0f;
        this.rotation = 0f;

        if (shapeType is ShapeType.Box)
        {
            vertices = CreateBoxVertices(width, height);
            Triangles = CreateBoxTriangles();
            transformedVertices = new  MonoVector[vertices.Length];
        }
        transformUpdateRequired = true;
    }

    public void MoveBy(MonoVector amount)
    {
        position += amount;
        transformUpdateRequired = true;
    }

    public void MoveTo(MonoVector position)
    {
        this.position = position;
        transformUpdateRequired = true;
    }

    public void Rotate(float amount)
    {
        rotation += amount;
        transformUpdateRequired = true;
    }

    /* --- Creation --- */
    public static bool CreateCircleBody(MonoVector position, float radius, float density, bool isStatic, float restitution, out RigidBody body, out string errorMessage)
    {
        body = null;
        errorMessage = string.Empty;

        float area = MathF.PI * radius * radius;
        if (area < World.MinBodySize || area > World.MaxBodySize)
        {
            errorMessage = $"Circle area outside limits specified by World. Current area: {area}, World Limits: ({World.MinBodySize}, {World.MaxBodySize}).";
            return false;
        }
        if (density < World.MinDensity || density > World.MaxDensity)
        {
            errorMessage = $"Circle density outside limits specified by World. Current density: {density}, World Limits: ({World.MinDensity}, {World.MaxDensity}).";
            return false;
        }
        
        restitution = Math.Clamp(restitution, 0f, 1f);
        float mass = density * area;
        
        body = new RigidBody(position, density, mass, restitution, area, isStatic, ShapeType.Circle, radius, 0f, 0f);
        return true;
    }
    
    public static bool CreateBoxBody(MonoVector position, float width, float height, float density, bool isStatic, float restitution, out RigidBody body, out string errorMessage)
    {
        body = null;
        errorMessage = string.Empty;

        float area = width * height;
        if (area < World.MinBodySize || area > World.MaxBodySize)
        {
            errorMessage = $"Box area outside limits specified by World. Current area: {area}, World Limits: ({World.MinBodySize}, {World.MaxBodySize}).";
            return false;
        }
        if (density < World.MinDensity || density > World.MaxDensity)
        {
            errorMessage = $"Box density outside limits specified by World. Current density: {density}, World Limits: ({World.MinDensity}, {World.MaxDensity}).";
            return false;
        }
        
        restitution = Math.Clamp(restitution, 0f, 1f);
        float mass = density * area;
        
        body = new RigidBody(position, density, mass, restitution, area, isStatic, ShapeType.Box, 0f, width, height);
        return true;
    }
    
    /* --- Vertices --- */
    public MonoVector[] GetTransformedVertices()
    {
        if (transformUpdateRequired)
        {
            BodyTransform transform = new BodyTransform(position, rotation);

            for (int i = 0; i < vertices.Length; i++)
            {
                transformedVertices[i] = vertices[i].Transform(transform);
            }
            
            transformUpdateRequired = false;
        }
        
        return transformedVertices;
    }
    
    public static MonoVector[] CreateBoxVertices(float width, float height)
    {
        float left = -width / 2f;
        float right = left + width;
        float bottom = -height / 2f;
        float top = bottom + height;

        MonoVector[] vertices = new MonoVector[4];
        vertices[0] = new MonoVector(left, top);
        vertices[1] = new MonoVector(right, top);
        vertices[2] = new MonoVector(right, bottom);
        vertices[3] = new MonoVector(left, bottom);
        
        return vertices;
    }

    /* --- Triangulation --- */
    public static int[] CreateBoxTriangles()
    {
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        
        return triangles;
    }
}