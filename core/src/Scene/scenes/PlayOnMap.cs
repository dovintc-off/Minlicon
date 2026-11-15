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

    private const double SimulationTickDuration = 1.0 / 60.0;
    private const double MaximumAccumulatedTime = 0.25;
    private double _tickAccumulator;

    public PlayOnMap(Main game) : base(game, "PlayOnMap") {
        Overlay = new Overlay();
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        KeyboardState kState = input.Keyboard;
        MouseState mState = input.Mouse;

        // Mouse position
        SetMousePosition(mState.Position.ToVector2());

        // Exit
        if (kState.IsKeyDown(Keys.Escape)) main.Exit();

        // Debug
        if (kState.IsKeyDown(Keys.F3) &&
            input.PreviousKeyboard.IsKeyUp(Keys.F3)) {
            main.ToggleDebug();
        }

        // Camera movement
        float baseSpeed = 400f;

        if (kState.IsKeyDown(Keys.LeftShift) ||
            kState.IsKeyDown(Keys.RightShift)) {
            baseSpeed *= 2.5f;
        }

        float cameraSpeed =
            baseSpeed *
            (1f / Zoom) *
            (float)gameTime.ElapsedGameTime.TotalSeconds;

        float x = 0f;
        float y = 0f;

        if (kState.IsKeyDown(Keys.A)) x -= cameraSpeed;
        if (kState.IsKeyDown(Keys.D)) x += cameraSpeed;
        if (kState.IsKeyDown(Keys.W)) y -= cameraSpeed;
        if (kState.IsKeyDown(Keys.S)) y += cameraSpeed;

        MoveCamera(x, y);

        // Zoom
        if (mState.ScrollWheelValue != input.PreviousMouse.ScrollWheelValue) {
            int delta = mState.ScrollWheelValue -
                        input.PreviousMouse.ScrollWheelValue;

            AdjustTargetZoom(Math.Sign(delta) * 0.15f);
        }
    }

    public override void Update(GameTime gt) {

        float lerpSpeed = 10f * (float)gt.ElapsedGameTime.TotalSeconds;

        _zoom = MathHelper.Lerp(_zoom, _targetZoom, MathHelper.Clamp(lerpSpeed, 0f, 1f));

        _tickAccumulator = Math.Min(
            _tickAccumulator + gt.ElapsedGameTime.TotalSeconds,
            MaximumAccumulatedTime
        );

        while (_tickAccumulator >= SimulationTickDuration) {

            _hoveredTile = new Point(
                (int)Math.Floor(
                    ((_mouseScreenPos.X - main.GraphicsDevice.Viewport.Width / 2f) / _zoom + _cameraX) / TileSize
                ),
                (int)Math.Floor(
                    ((_mouseScreenPos.Y - main.GraphicsDevice.Viewport.Height / 2f) / _zoom + _cameraY) / TileSize
                )
            );
            _tickAccumulator -= SimulationTickDuration;
        }

        _cameraX = MathHelper.Clamp(_cameraX, 0f, MapWidth * TileSize);
        _cameraY = MathHelper.Clamp(_cameraY, 0f, MapHeight * TileSize);

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
#if WINDOWS
        atlasGenerator.SaveAtlas("atlas.png");
#endif
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