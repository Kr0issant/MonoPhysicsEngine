using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoUtils;
using MonoUtils.Graphics;
using MonoUtils.Input;
using MonoUtils.Utility;

namespace MonoPhysicsEngine;

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

    private float x = 0;

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
        
        if (keyboard.IsKeyClicked(Keys.Escape)) { Exit(); }
        if (keyboard.IsKeyClicked(Keys.F)) { screen.ToggleFullScreen(graphics); }
        
        if (mouse.IsMiddleButtonDown()) { camera.MoveCam(new Vector2(-mouse.DeltaX * camera.ZoomFactor, mouse.DeltaY * camera.ZoomFactor)); }
        if (mouse.IsScrollingUp()) { camera.MoveZ(35f * camera.ZoomFactor); }
        if (mouse.IsScrollingDown()) { camera.MoveZ(-35f * camera.ZoomFactor); }


        x += 0.05f;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        screen.Set();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        sprites.Begin(camera, false);
        sprites.End();
        
        shapes.Begin(camera);
        shapes.DrawRectangle(20f, 0f, 100f, -50f, Color.Red);
        shapes.DrawRectangle(20f, 0f, 100f, -50f, Color.Black, Shapes.FillMode.Border, 4f);
        shapes.DrawCircle(new Vector2(0f, 0f), 50f, Color.White);
        shapes.DrawRegularPolygon(new Transform2D(new Vector2(0f, 0f), new Vector2(50, 50), x), 5, Color.Red);
        shapes.End();
        
        screen.Unset();
        screen.Present(sprites);

        base.Draw(gameTime);
    }
}