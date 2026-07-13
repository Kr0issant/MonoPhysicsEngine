using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoUtils;
using MonoUtils.Graphics;
using MonoUtils.Input;
using MonoUtils.Utility;
using MonoPhysicsEngine;
using MonoPhysicsEngine.Content;
using Util = MonoPhysicsEngine.Util;

namespace MonoPhysicsEngineTester;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteFont font;
    private Shapes shapes;
    private Sprites sprites;
    private Camera camera;
    private Screen screen;
    private UtilsKeyboard keyboard = new  UtilsKeyboard();
    private UtilsMouse mouse =  new  UtilsMouse();
    
    private const int SCREEN_WIDTH = 1280;
    private const int SCREEN_HEIGHT = 720;

    private const int DEFAULT_ZOOM = 4;

    private Color SHAPE_BORDER_COLOR = Color.Black;
    
    private List<RigidBody> bodies;
    private List<Color> colors;
    private Vector2[] vertexBuffer;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
        graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
        graphics.ApplyChanges();
        
        Window.AllowUserResizing = true;
        
        shapes = new Shapes(this);
        sprites = new Sprites(this);
        screen = new Screen(this, SCREEN_WIDTH, SCREEN_HEIGHT);
        camera = new Camera(screen);

        camera.Zoom = DEFAULT_ZOOM;
        camera.GetScreenBounds(out float left, out float right, out float bottom, out float top);
        // Console.WriteLine(left);
        // Console.WriteLine(right);
        // Console.WriteLine(top);
        // Console.WriteLine(bottom);
        
        float padding = 20f;
        int numBodies = 10;
        bodies = new List<RigidBody>(numBodies);
        colors = new List<Color>(numBodies);
        Random rng = new Random();
        
        for (int i = 0; i < numBodies; i++)
        {
            RigidBody body = null;
            string msg = null;
            bool success = false;
            
            // int type = rng.Next(0, 2);
            int type = (int)ShapeType.Box;
            
            int x = rng.Next((int)(left + padding), (int)(right - padding));
            int y = rng.Next((int)(bottom + padding), (int)(top - padding));
            Color color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            // double area = rng.NextDouble() * (World.MaxBodySize - World.MinBodySize) + World.MinBodySize;
            double area = 15f * 15f;
            float density = (float)(rng.NextDouble() * (World.MaxDensity - World.MinDensity) + World.MinDensity);
            
            if (type == 0)
            {
                float radius = MathF.Sqrt((float)area / MathF.PI);
                success = RigidBody.CreateCircleBody(new MonoVector(x, y), radius, density, false, 1f, out body, out msg);
            }
            else if (type == 1)
            {
                float maxDimensions = MathF.Sqrt((float)area);
                float width = maxDimensions * (float)(rng.NextDouble() * 1.0 + 0.5);
                float height = (float)area / width;
        
                success = RigidBody.CreateBoxBody(new MonoVector(x, y), width, height, density, false, 1f, out body, out msg);
            }
            
            if (success) { bodies.Add(body); colors.Add(color); }
            else Console.WriteLine($"[{i}] {msg}");
        }
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        font = Content.Load<SpriteFont>("font");
        sprites.Font = font;
    }

    protected override void Update(GameTime gameTime)
    {
        /* --- Input Handling --- */
        
        keyboard.Update();
        mouse.Update();
        
        // Console.WriteLine(camera.Z);
        float zoomFactor = (float)(camera.Z / camera.ZBase);
        
        if (keyboard.IsKeyClicked(Keys.Escape)) { Exit(); }
        if (keyboard.IsKeyClicked(Keys.F)) { screen.ToggleFullScreen(graphics); }
        
        if (mouse.IsMiddleButtonDown()) { camera.MoveBy(new Vector2(-mouse.DeltaX, mouse.DeltaY) * zoomFactor); }
        if (mouse.IsScrollingDown()) { camera.MoveZ(35f * zoomFactor); }
        if (mouse.IsScrollingUp()) { camera.MoveZ(-35f * zoomFactor); }
        
        if (keyboard.IsKeyDown(Keys.Left)) { camera.MoveBy(new Vector2(-8f, 0f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Right)) { camera.MoveBy(new Vector2(8f, 0f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Up)) { camera.MoveBy(new Vector2(0f, 8f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Down)) { camera.MoveBy(new Vector2(0f, -8f) * zoomFactor); }
        if (keyboard.IsKeyClicked(Keys.R)) { camera.ResetZ(); camera.MoveBy(-camera.Position); }

        float dx = 0f;
        float dy = 0f;
        float speed = 32f;
        
        if (keyboard.IsKeyDown(Keys.A)) { dx--; }
        if (keyboard.IsKeyDown(Keys.D)) { dx++; }
        if (keyboard.IsKeyDown(Keys.W)) { dy++; }
        if (keyboard.IsKeyDown(Keys.S)) { dy--; }

        if (mouse.IsLeftButtonDown())
        {
            Vector2 diff = mouse.GetScreenPosition(screen, camera) - bodies[0].Position.ToVector2();
            if (diff.Length() > new Vector2(0.1f, 0.1f).Length())
            {
                dx = diff.X;
                dy = diff.Y;
            }
        }
        
        /* --- Movement --- */
        
        if (dx != 0 || dy != 0)
        {
            MonoVector direction = new MonoVector(dx, dy).Normalize();
            MonoVector velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            bodies[0].MoveBy(velocity);
        }
        
        /* --- Collision Detection --- */

        // for (int i = 0; i < bodies.Count; i++)
        // {
        //     RigidBody bodyA = bodies[i];
        //     for (int j = i + 1; j < bodies.Count; j++)
        //     {
        //         RigidBody bodyB = bodies[j];
        //
        //         if (Collisions.CheckCircleCollision(bodyA.Position, bodyB.Position, bodyA.Radius, bodyB.Radius, out MonoVector normal, out float depth))
        //         {
        //             bodyA.MoveBy(normal * depth * (bodyB.Mass / (bodyA.Mass + bodyB.Mass)));
        //             bodyB.MoveBy(-normal * depth * (bodyA.Mass / (bodyA.Mass + bodyB.Mass)));
        //         }
        //     }
        // }
        
        /* --- Collision Resolution --- */
        // for (int i = 0; i < bodies.Count; i++)
        // {
        //     RigidBody body = bodies[i];
        //     body.MoveBy(body.LinearVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
        //     body.AddVelocity(-body.LinearVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
        // }
        
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        screen.Set();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        sprites.Begin(camera, false);
        sprites.End();
        
        shapes.Begin(camera);
        for (int i = 0; i < bodies.Count; i++)
        {
            RigidBody body = bodies[i];
            ShapeType type = body.ShapeType;
            if (type == ShapeType.Circle)
            {
                shapes.DrawCircle(body.Position.ToVector2(), body.Radius, colors[i], Shapes.FillMode.Filled);
                shapes.DrawCircle(body.Position.ToVector2(), body.Radius, SHAPE_BORDER_COLOR, Shapes.FillMode.Border);
            }
            else if (type == ShapeType.Box)
            {
                Util.ToVector2Array(body.GetTransformedVertices(), ref vertexBuffer);
                shapes.DrawPolygon(vertexBuffer, body.Triangles, colors[i], Shapes.FillMode.Filled);
                shapes.DrawPolygon(vertexBuffer, body.Triangles, SHAPE_BORDER_COLOR, Shapes.FillMode.Border);
            }
        }
        // shapes.DrawLine(Vector2.Zero, new MonoVector(2f, 1f).ToVector2(), 0.01f, Color.White);
        // shapes.DrawCircle();
        shapes.End();
        
        screen.Unset();
        screen.Present(sprites);

        base.Draw(gameTime);
    }
}