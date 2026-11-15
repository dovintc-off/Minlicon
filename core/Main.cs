using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.TileSystem;
using Minlicon.Core.Graphics;

namespace Minlicon.Core;

public partial class Main : Game {
    protected readonly SceneManager _sceneManager = new();
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch? _spriteBatch;

    protected const int MapWidth = 100;
    protected const int MapHeight = 100;
    protected const int TileSize = 32;

    protected TileSystem.TileSystem _tileSystem = null!;
    protected TileRegistry _tileRegistry = null!;
    protected AtlasGenerator.AtlasType _globalAtlas = null!;
    protected WorldDrawer _worldDrawer = null!;

    protected float WorldCameraX = 0f;
    protected float WorldCameraY = 0f;

    protected Texture2D? _cursorTexture;
    protected Point _hoveredTile = new Point(-1, -1);
    protected Vector2 _mouseScreenPos;

    protected const double SimulationTickDuration = 1.0 / 60.0;
    protected const double MaximumAccumulatedTime = 0.25;
    protected double _tickAccumulator;

    public Main() {
        _graphics = new GraphicsDeviceManager(this);
        IsFixedTimeStep = false;
        TargetElapsedTime = TimeSpan.FromSeconds(SimulationTickDuration);
        _graphics.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize() {
        ConfigurePlatformGraphics();
        _graphics.ApplyChanges();

        InitializeInput();
        _fpsWindowStartTimestamp = Stopwatch.GetTimestamp();

        WorldCameraX = (MapWidth * TileSize) / 2f;
        WorldCameraY = (MapHeight * TileSize) / 2f;

        _pixelBackground = new Texture2D(GraphicsDevice, 1, 1);
        _pixelBackground.SetData(new[] { Color.White });

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        FontRegistry.Initialize();

        Dictionary<string, string> rawJsonData = LoadRegistryData();
        _tileRegistry = new TileRegistry(rawJsonData);
        
        AtlasGenerator atlasGenerator = new AtlasGenerator(Content, GraphicsDevice);
        atlasGenerator.BakeRegistry(_tileRegistry);
        atlasGenerator.SaveAtlas("atlas.png");
        _globalAtlas = atlasGenerator.GetAtlas();
        
        GroundDefinition defaultGround = _tileRegistry.Grounds["Grass"];
        _tileSystem = new TileSystem.TileSystem(MapWidth, MapHeight, defaultGround);
        
        _worldDrawer = new WorldDrawer(_spriteBatch, _tileSystem, _globalAtlas, _tileRegistry, TileSize);
        _cursorTexture = Content.Load<Texture2D>("assets/Cursor");

        for (int i = 1; i<MapWidth; i++){
            _tileSystem.Set(i, 0, new TileCell(_tileRegistry.Grounds["Sand"], null, 90, 0));
            System.Console.Write(_tileSystem.Get(2, 2).Rotate);
            System.Console.WriteLine(_tileSystem.Get(2, 2).TextureVariant);
        }

        _sceneManager.Reg(new GameScene(this));
        _sceneManager.Next("Game");
    }

    protected override void Update(GameTime gameTime) {
        UpdateInput(gameTime);
        _sceneManager.Update(gameTime);
        base.Update(gameTime);
    }

    internal void DrawWorld() {
        _worldDrawer.Draw(
            WorldCameraX,
            WorldCameraY,
            Zoom,
            GraphicsDevice.Viewport,
            _hoveredTile
        );
    }
}

public class GameScene : Scene {
    public GameScene(Main game) : base(game, "Game") {}

    public override void Draw(GameTime gt) {
        main.DrawWorld();

        base.Draw(gt);
    }

    public override void Update(GameTime gt) {
        base.Update(gt);
    }
}

public class SceneManager {
    private readonly Dictionary<string, Scene> _scenes = new();

    public Scene? ActualScene {get; private set;}

    public void Reg(Scene _newScene) {
        _scenes[_newScene.Name] = _newScene;
    }

    public void Next(string name) {
        if (!_scenes.TryGetValue(name, out var next)) {
            throw new Exception($"Scene {name} is not found");
        }
        if (ActualScene == next) return;
        ActualScene?.Exit(); ActualScene?.Unload();
        ActualScene = next;
        ActualScene.Load(); ActualScene.Enter();
    }

    public void Draw(GameTime gt) {
        if (ActualScene == null) throw new Exception($"ActualScene is null");
        ActualScene.Draw(gt);
    }

    public void Update(GameTime gt) {
        if (ActualScene == null) throw new Exception($"ActualScene is null");
        ActualScene.Update(gt);
    }
}

public abstract class Scene {
    public Overlay? Overlay {get; protected set;}
    protected Main main {get;}
    public string Name {get; private set;}

    protected Scene(Main game, string name) {
        main = game;
        Name = name;
    }

    public virtual void Unload() {}
    public virtual void Load() {}
    public virtual void Exit() {}
    public virtual void Enter() {}
    public virtual void Update(GameTime gt) {
        Overlay?.Update(gt);
    }
    public virtual void Draw(GameTime gt) {
        Overlay?.Draw(gt);
    }
}

public class Overlay {
    public UIManger UI {get;}

    public Overlay() {
        UI = new UIManger();
    }

    public void Update(GameTime gt) {
        UI.Update(gt);
    }

    public void Draw(GameTime gt) {
        UI.Draw(gt);
    }
}

public class UIManger {
    private readonly List<UIObject> UIObjects = new();

    public void New(UIObject obj) {
        UIObjects.Add(obj);
    }

    public void Update(GameTime gt){
        foreach (UIObject obj in UIObjects) {
            obj.Update(gt);
        }
    }

    public void Draw(GameTime gt){
        foreach (UIObject obj in UIObjects) {
            obj.Draw(gt);
        }
    }
}

public abstract class UIObject {
    public virtual void Update(GameTime gt) {}
    public virtual void Draw(GameTime gt) {}
}

public class InputUI : UIObject {
    
}