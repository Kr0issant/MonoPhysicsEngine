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

    private World world;
    private float timeScale;

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

        world = new World();
        Util.SpawnRandomBodies(world, 100, camera, SHAPE_BORDER_COLOR, area: 15f * 15f);
        
        timeScale = 1.0f;
        
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
        
        if (keyboard.IsKeyClicked(Keys.OemPlus)) { timeScale = Math.Clamp(timeScale * 2f, 0.25f, 4f); }
        if (keyboard.IsKeyClicked(Keys.OemMinus)) { timeScale = Math.Clamp(timeScale / 2f, 0.25f, 4f); }
        
        float dx = 0f;
        float dy = 0f;
        float forceMagnitude = 48f;
        
        if (keyboard.IsKeyDown(Keys.A)) { dx--; }
        if (keyboard.IsKeyDown(Keys.D)) { dx++; }
        if (keyboard.IsKeyDown(Keys.W)) { dy++; }
        if (keyboard.IsKeyDown(Keys.S)) { dy--; }
        
        if (mouse.IsLeftButtonDown())
        {
            Vector2 diff = mouse.GetScreenPosition(screen, camera) - world.GetBody(0).Position.ToVector2();
            if (diff.Length() > new Vector2(0.1f, 0.1f).Length())
            {
                dx = diff.X;
                dy = diff.Y;
            }
        }
        if (dx != 0 || dy != 0)
        {
            MonoVector forceDirection = new MonoVector(dx, dy).Normalize();
            MonoVector force = forceDirection * forceMagnitude;
            world.GetBody(0).AddForce(force);
        }

        world.Step((float)gameTime.ElapsedGameTime.TotalSeconds * timeScale);
        
        Util.WrapScreen(camera, world);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        screen.Set();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        shapes.Begin(camera);
        world.DrawShapes(shapes, world);
        shapes.End();

        camera.GetScreenBounds(out float left, out float right, out float bottom, out float top);
        float zoomFactor = (float)(camera.Z / camera.ZBase);
        Vector2 topRight = new Vector2(left + 12f * zoomFactor, top - 30f * zoomFactor);
        
        sprites.Begin(camera, false);
        sprites.DrawString($"Time Scale: {MathF.Round(timeScale, 2)}x", topRight, Color.DarkGreen, 1.2f * zoomFactor);
        sprites.End();
        
        screen.Unset();
        screen.Present(sprites);

        base.Draw(gameTime);
    }
}