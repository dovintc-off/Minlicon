using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Minlicon.Core.TileSystem;
using Minlicon.Core.Graphics;
using Minlicon.Core.Scenes;
using Minlicon.Core;

public class PlayOnMap : Scene {
    public Texture2D? CursorTexture => _cursorTexture;
    public Vector2 MouseScreenPosition => _mouseScreenPos;

    internal float CameraX => _cameraX;
    internal float CameraY => _cameraY;
    internal Point HoveredTile => _hoveredTile;

    private const int MapWidth = 100;
    private const int MapHeight = 100;
    private const int TileSize = 32;

    private TileSystem _tileSystem = null!;
    private TileRegistry _tileRegistry = null!;
    private AtlasGenerator.AtlasType _globalAtlas = null!;
    private WorldDrawer _worldDrawer = null!;

    private float _cameraX;
    private float _cameraY;

    private float _zoom = 1f;
    private float _targetZoom = 1f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 1.5f;

    private Texture2D? _cursorTexture;
    private Point _hoveredTile = new(-1, -1);
    private Vector2 _mouseScreenPos;

    private MouseState _prevMouseState;
    private KeyboardState _prevKeyboardState;

    private double _tickAccumulator;

    public PlayOnMap(Main game) : base(game, "PlayOnMap") {
        Overlay = new Overlay();
    }

    public override void Update(GameTime gt) {
        base.Update(gt);
    }

    public override void Load() {
        Dictionary<string, string> rawJsonData = main.GetRegistryData();

        _tileRegistry = new TileRegistry(rawJsonData);

        AtlasGenerator atlasGenerator = new AtlasGenerator(
            main.Content,
            main.GraphicsDevice
        );

        atlasGenerator.BakeRegistry(_tileRegistry);
        atlasGenerator.SaveAtlas("atlas.png");

        _globalAtlas = atlasGenerator.GetAtlas();

        GroundDefinition defaultGround = _tileRegistry.Grounds["Grass"];

        _tileSystem = new TileSystem(
            MapWidth,
            MapHeight,
            defaultGround
        );

        _worldDrawer = new WorldDrawer(
            main.SpriteBatch,
            _tileSystem,
            _globalAtlas,
            _tileRegistry,
            TileSize
        );

        _cursorTexture = main.Content.Load<Texture2D>("assets/Cursor");

        _cameraX = (MapWidth * TileSize) / 2f;
        _cameraY = (MapHeight * TileSize) / 2f;

        _prevMouseState = Mouse.GetState();
        _prevKeyboardState = Keyboard.GetState();

        for (int i = 1; i < MapWidth; i++) {
            _tileSystem.Set(
                i, 0,
                new TileCell(_tileRegistry.Grounds["Sand"], null, 90, 0)
            );
        }
    }

    public override void Draw(GameTime gt) {
        SpriteBatch spriteBatch = main.SpriteBatch;

        float centerX = main.GraphicsDevice.Viewport.Width / 2f;
        float centerY = main.GraphicsDevice.Viewport.Height / 2f;

        Matrix cameraMatrix =
            Matrix.CreateTranslation(new Vector3(-_cameraX, -_cameraY, 0)) *
            Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(centerX, centerY, 0));

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            cameraMatrix
        );

        _worldDrawer.Draw(
            _cameraX,
            _cameraY,
            _zoom,
            main.GraphicsDevice.Viewport,
            _hoveredTile
        );

        spriteBatch.End();

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp
        );

        base.Draw(gt);

        spriteBatch.End();
    }

    public float Zoom => _zoom;

    public void MoveCamera(float x, float y) {
        _cameraX += x;
        _cameraY += y;
    }

    public void AdjustTargetZoom(float amount) {
        _targetZoom = MathHelper.Clamp(
            _targetZoom + amount,
            MinZoom,
            MaxZoom
        );
    }

    public void SetMousePosition(Vector2 position) {
        _mouseScreenPos = position;
    }
}