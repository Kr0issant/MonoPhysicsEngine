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
        Console.WriteLine(left);
        Console.WriteLine(right);
        Console.WriteLine(top);
        Console.WriteLine(bottom);
        
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
            
            int type = rng.Next(0, 2);
            // int type = (int)ShapeType.Circle;
            
            int x = rng.Next((int)(left + padding), (int)(right - padding));
            int y = rng.Next((int)(bottom + padding), (int)(top - padding));
            Color color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            double area = rng.NextDouble() * (World.MaxBodySize - World.MinBodySize) + World.MinBodySize;
            float density = (float)(rng.NextDouble() * (World.MaxDensity - World.MinDensity) + World.MinDensity);
            
            if (type == 0)
            {
                float radius = MathF.Sqrt((float)area / MathF.PI);
                success = RigidBody.CreateCircleBody(new MonoVector(x, y), radius, density, false, 1f, out body, out msg);
            }
            else if (type == 1)
            {
                float maxDimen = MathF.Sqrt((float)area);
                float width = maxDimen * (float)(rng.NextDouble() * 1.0 + 0.5);
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
        keyboard.Update();
        mouse.Update();
        
        // Console.WriteLine(camera.Z);
        float zoomFactor = (float)(camera.Z / camera.ZBase);
        
        if (keyboard.IsKeyClicked(Keys.Escape)) { Exit(); }
        if (keyboard.IsKeyClicked(Keys.F)) { screen.ToggleFullScreen(graphics); }
        
        if (mouse.IsMiddleButtonDown()) { camera.MoveBy(new Vector2(-mouse.DeltaX, mouse.DeltaY) * zoomFactor); }
        if (mouse.IsScrollingUp()) { camera.MoveZ(35f * zoomFactor); }
        if (mouse.IsScrollingDown()) { camera.MoveZ(-35f * zoomFactor); }
        
        if (keyboard.IsKeyDown(Keys.Left)) { camera.MoveBy(new Vector2(-8f, 0f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Right)) { camera.MoveBy(new Vector2(8f, 0f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Up)) { camera.MoveBy(new Vector2(0f, 8f) * zoomFactor); }
        if (keyboard.IsKeyDown(Keys.Down)) { camera.MoveBy(new Vector2(0f, -8f) * zoomFactor); }
        if (keyboard.IsKeyClicked(Keys.R)) { camera.ResetZ(); camera.MoveBy(-camera.Position); }
        
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
                shapes.DrawRectangle(body.Position.ToVector2(), body.Width, body.Height, colors[i], Shapes.FillMode.Filled);
                shapes.DrawRectangle(body.Position.ToVector2(), body.Width, body.Height, SHAPE_BORDER_COLOR, Shapes.FillMode.Border);
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