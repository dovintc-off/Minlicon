using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.Graphics;
using Minlicon.Core.Scenes;
using Microsoft.Xna.Framework.Input.Touch;

namespace Minlicon.Core;

public partial class Main : Game {
    internal SpriteBatch SpriteBatch => _spriteBatch ?? throw new InvalidOperationException("SpriteBatch is not initialized");
    internal Dictionary<string, string> GetRegistryData() => LoadRegistryData();

    protected readonly SceneManager _sceneManager = new();
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch? _spriteBatch;

    public Main() {
        _graphics = new GraphicsDeviceManager(this);
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize() {
        ConfigurePlatformGraphics();
        _graphics.ApplyChanges();

        InitializeInput();
        _fpsWindowStartTimestamp = Stopwatch.GetTimestamp();

        _pixelBackground = new Texture2D(GraphicsDevice, 1, 1);
        _pixelBackground.SetData(new[] { Color.White });

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        FontRegistry.Initialize();
        _sceneManager.Reg(new PlayOnMap(this));
        _sceneManager.Next("PlayOnMap");
    }

    protected override void Update(GameTime gameTime) {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();

        InputState input = new InputState(
            keyboard, mouse, _previousKeyboardState, _previousMouseState
        );

        HandlePlatformInput(gameTime, keyboard, mouse);

        _sceneManager.ActualScene?.HandleInput(gameTime, input);

        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;

        _sceneManager.Update(gameTime);

        base.Update(gameTime);
    }
}

// проще говоря этот класс приводит управление с windows и android в один готовый вид
public static class GameInput {
    public static float zoomDelta {get; private set;}
    private static MouseState _prevm;
    private static TouchCollection _prevt;

    public static event Action<PressedData>? OnPressed; // для телефона
    public static event Action<PressedData>? OnPressedLMB;
    public static event Action<PressedData>? OnPressedRMB;
    public static event Action<PressedData>? OnPressedMMB;
    public static event Action<PressedData>? OnPressed1MB;
    public static event Action<PressedData>? OnPressed2MB;
    private static PressedData?[] _pressed = new PressedData?[5];

    public static event Action<ReleasedData>? OnReleased; // для телефона
    public static event Action<ReleasedData>? OnReleasedLMB;
    public static event Action<ReleasedData>? OnReleasedRMB;
    public static event Action<ReleasedData>? OnReleasedMMB;
    public static event Action<ReleasedData>? OnReleased1MB;
    public static event Action<ReleasedData>? OnReleased2MB;
    private static ReleasedData?[] _released = new ReleasedData?[5];

    public static event Action<HeldData>? HeldUpdate; // для телефона
    public static event Action<HeldData>? HeldUpdateLMB;
    public static event Action<HeldData>? HeldUpdateRMB;
    public static event Action<HeldData>? HeldUpdateMMB;
    public static event Action<HeldData>? HeldUpdate1MB;
    public static event Action<HeldData>? HeldUpdate2MB;
    public static event Action<HeldData>? HeldEnd; // также
    public static event Action<HeldData>? HeldEndLMB;
    public static event Action<HeldData>? HeldEndRMB;
    public static event Action<HeldData>? HeldEndMMB;
    public static event Action<HeldData>? HeldEnd1MB;
    public static event Action<HeldData>? HeldEnd2MB;
    private static HeldData?[] _held = new HeldData?[5];

    public static void Update(MouseState mouse, GameTime gt) {
        UpdateZoom(mouse, gt);
        _prevm = mouse;
    }

    public static void Update(TouchCollection touches, GameTime gt){
        UpdateZoom(touches, gt);
        _prevt = touches;
    }

    // здесь вычесления изменения
    private static void UpdateZoom(MouseState mouse, GameTime gt) {}
    private static void UpdateZoom(TouchCollection touches, GameTime gt) {}
}

// ClickedBy может быть Null т.к. на телефоне нажимается пальцем, Null будет означать палец
public class PressedData {
    public Point Pos;
    public ClickState? ClickedBy;
}

public class HeldData {
    public Point Pos;
    public Point PosStart;
    public float Duration; // сколько всего зажато. в секундах
    public ClickState? ClickedBy;
}

public class ReleasedData {
    public Point Pos;
    public ClickState? ClickedBy;
}

public enum ClickState {
    RMB, LMB, MMB, _1MB, _2MB
}