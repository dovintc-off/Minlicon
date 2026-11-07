using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.TileSystem;
using Minlicon.Core.Graphics;

namespace Minlicon.Core;

public partial class Main : Game {
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
#if !ANDROID
        _graphics.SynchronizeWithVerticalRetrace = false;
#endif
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
    }

    protected override void Update(GameTime gameTime) {
        UpdateInput(gameTime);
        base.Update(gameTime);
    }
}
