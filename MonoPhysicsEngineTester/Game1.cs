using System;
using System.Diagnostics;
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
using MonoGame.ImGui.Standard;

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
    
    private TesterGUI testerGui;
    
    private const int SCREEN_WIDTH = 1280;
    private const int SCREEN_HEIGHT = 720;

    private const int DEFAULT_ZOOM = 4;

    private Color BACKGROUND_COLOR = Color.CornflowerBlue;
    private Color SHAPE_BORDER_COLOR = Color.Black;
    private Color SHAPE_STATIC_BORDER_COLOR = Color.LightGray;
    // private Color SHAPE_FILL_COLOR = Color.White;
    private Color SHAPE_STATIC_FILL_COLOR = new Color(80, 80, 80);

    private bool RENDER_AABBs = true;
    private bool FPS_CAP_60 = true;
    
    private World world;
    private float timeScale;
    private bool isPaused;
    
    private FrameCounter frameCounter;
    private FrameCounter tickCounter;
    
    private Stopwatch watch;

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
        if (!FPS_CAP_60)
        {
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }
        graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
        graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
        graphics.ApplyChanges();
        
        Window.AllowUserResizing = true;
        
        frameCounter = new FrameCounter();
        tickCounter = new FrameCounter();
        
        shapes = new Shapes(this);
        sprites = new Sprites(this);
        screen = new Screen(this, SCREEN_WIDTH, SCREEN_HEIGHT);
        camera = new Camera(screen);

        camera.Zoom = DEFAULT_ZOOM;
        camera.GetScreenBounds(out float width, out float height);

        world = new World();
        Util.SpawnRandomBodies(world, 5, camera, SHAPE_BORDER_COLOR, SHAPE_STATIC_FILL_COLOR, SHAPE_STATIC_BORDER_COLOR, area: 15f * 15f);
        // Util.SpawnRandomBodies(world, 4, camera, SHAPE_BORDER_COLOR, SHAPE_STATIC_FILL_COLOR, SHAPE_STATIC_BORDER_COLOR, area: 15f * 15f, isStatic:true);
        
        float padding = width * 0.1f;
        if (RigidBody.CreateBoxBody(new MonoVector(0, -height / 3f), width - padding, 8f, 1f, true, 1f, Shapes.FillMode.Filled, SHAPE_STATIC_FILL_COLOR, SHAPE_STATIC_BORDER_COLOR,  out RigidBody body, out string msg))
        {
            world.AddBody(body);
        }
        
        timeScale = 1.0f;
        isPaused = false;

        // testerGui = new TesterGUI(world, camera, SHAPE_STATIC_FILL_COLOR, SHAPE_BORDER_COLOR, SHAPE_STATIC_BORDER_COLOR);
        // testerGui.Initialize(this);
        
        watch = new Stopwatch();
        
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
        tickCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        
        keyboard.Update();
        mouse.Update();

        // bool mouseOnGui = ImGuiNET.ImGui.GetIO().WantCaptureMouse;
        
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

        if (keyboard.IsKeyClicked(Keys.OemTilde))
        {
            Console.WriteLine($"{world.BodyCount} bodies");
            Console.WriteLine($"tick time: {Math.Round(watch.Elapsed.TotalMilliseconds, 4)} ms");
        }
        
        MonoVector mousePos = MonoVector.FromVector2(mouse.GetScreenPosition(screen, camera));
        
        if (mouse.IsLeftButtonClicked())
        {
            Util.SpawnRandomBodyAt(world, mousePos, SHAPE_BORDER_COLOR, SHAPE_STATIC_FILL_COLOR, SHAPE_STATIC_BORDER_COLOR, out string msg, isStatic: false, area: 15f * 15f, shapeType: ShapeType.Circle);
        }
        if (mouse.IsRightButtonClicked())
        {
            Util.SpawnRandomBodyAt(world, mousePos, SHAPE_BORDER_COLOR, SHAPE_STATIC_FILL_COLOR, SHAPE_STATIC_BORDER_COLOR, out string msg, isStatic: false, area: 15f * 15f, shapeType: ShapeType.Box);
        }
        
        // float dx = 0f;
        // float dy = 0f;
        // float forceMagnitude = 100f;
        //
        // if (keyboard.IsKeyDown(Keys.A)) { dx--; }
        // if (keyboard.IsKeyDown(Keys.D)) { dx++; }
        // if (keyboard.IsKeyDown(Keys.W)) { dy++; }
        // if (keyboard.IsKeyDown(Keys.S)) { dy--; }
        //
        // if (dx != 0 || dy != 0)
        // {
        //     MonoVector forceDirection = new MonoVector(dx, dy).Normalize();
        //     MonoVector force = forceDirection * forceMagnitude * world.GetBody(0).Mass;
        //     world.GetBody(0).AddForce(force);
        // }

        watch.Restart();
        if (!isPaused) world.Step((float)gameTime.ElapsedGameTime.TotalSeconds * timeScale);
        watch.Stop();
        
        // Util.WrapScreen(camera, world);

        camera.GetScreenBounds(out _, out _, out float bottom, out _);
        for (int i = world.BodyCount - 1; i >= 0; i--)
        {
            RigidBody body = world.GetBody(i);
            MonoAABB box = body.GetAABB();

            if (box.Max.Y < bottom) { world.RemoveBody(body); }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        screen.Set();
        GraphicsDevice.Clear(BACKGROUND_COLOR);
        
        frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        
        shapes.Begin(camera);
        world.DrawShapes(shapes, world, RENDER_AABBs);
        shapes.End();

        camera.GetScreenBounds(out float left, out float right, out float bottom, out float top);
        float zoomFactor = (float)(camera.Z / camera.ZBase);
        Vector2 topRight = new Vector2(left + 12f * zoomFactor, top - 30f * zoomFactor);
        Vector2 topRight2 = new Vector2(left + 12f * zoomFactor, top - 50f * zoomFactor);
        Vector2 topRight3 = new Vector2(left + 12f * zoomFactor, top - 70f * zoomFactor);
        
        sprites.Begin(camera, false);
        sprites.DrawString($"Time Scale: {MathF.Round(timeScale, 2)}x", topRight, Color.DarkGreen, 1.2f * zoomFactor);
        sprites.DrawString($"FPS: {MathF.Round(frameCounter.CurrentFramesPerSecond, 2)}", topRight2, Color.DarkGreen, 1.2f * zoomFactor);
        sprites.DrawString($"TPS: {MathF.Round(tickCounter.CurrentFramesPerSecond, 2)}", topRight3, Color.DarkGreen, 1.2f * zoomFactor);
        sprites.End();
        
        // testerGui.Draw(gameTime);
        
        screen.Unset();
        screen.Present(sprites);

        base.Draw(gameTime);
    }
}