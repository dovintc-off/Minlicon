namespace Minlicon.Core;

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

public enum Grounds {
    Grass = 1,
    UMad = -1
}
    
public class Main : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    private const int MapWidth = 100;
    private const int MapHeight = 100;
    private const int TileSize = 32;

    private readonly int[,,] GLayer = new int[MapWidth, MapHeight, 3];
    int[,] BLayer = new int[MapWidth, MapHeight];
    private readonly Texture2D?[] _grassTextures = new Texture2D?[5];
    private Texture2D? _specialTexture;

    float WorldCameraX = 0f;
    float WorldCameraY = 0f;

    float Zoom = 1.0f;
    private float _targetZoom = 1.0f;
    private float _minZoom = 0.5f;
    private float _maxZoom = 1.5f;

    private Texture2D? _cursorTexture;
    private Point _hoveredTile = new Point(-1, -1);
    private Vector2 _mouseScreenPos;

    private SpriteFont? _debugFont;
    private bool _showDebug = false;
    private double _fps;
    private double _tickAccumulator;
    private long _fpsWindowStartTimestamp;
    private int _renderedFrames;
    private string _fpsText = "FPS: 0";
    private Texture2D? _pixelBackground;

    private const double SimulationTickDuration = 1.0 / 60.0;
    private const double MaximumAccumulatedTime = 0.25;

    MouseState _prevMouseState;
    KeyboardState _prevKeyboardState;
    private int _previousTouchId = -1;
    private Vector2 _previousTouchPosition;
    private float _previousPinchDistance;

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
        // Dynamic adaptation
#if ANDROID
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        this.Window.Title = "Minlicon";
#endif
        _graphics.ApplyChanges();

        TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Pinch;

        Random rng = new Random();
        int[] customAngles = { 0, 90, 180, 270 };

        for (int i = 0; i < MapWidth; i++) {
            for (int j = 0; j < MapHeight; j++) {
                GLayer[i, j, 0] = (int)Grounds.Grass;
                GLayer[i, j, 1] = rng.Next(1, 5);
                GLayer[i, j, 2] = customAngles[rng.Next(customAngles.Length)];
            }
        }

        GLayer[50, 50, 0] = -1;
        _prevMouseState = Mouse.GetState();
        _prevKeyboardState = Keyboard.GetState();
        _fpsWindowStartTimestamp = Stopwatch.GetTimestamp();

        WorldCameraX = (MapWidth * TileSize) / 2f;
        WorldCameraY = (MapHeight * TileSize) / 2f;

        _pixelBackground = new Texture2D(GraphicsDevice, 1, 1);
        _pixelBackground.SetData(new[] { Color.White });

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("DebugFont");

        _specialTexture = Content.Load<Texture2D>("UMad");
        _grassTextures[1] = Content.Load<Texture2D>("Grass_1");
        _grassTextures[2] = Content.Load<Texture2D>("Grass_2");
        _grassTextures[3] = Content.Load<Texture2D>("Grass_3");
        _grassTextures[4] = Content.Load<Texture2D>("Grass_4");
        _cursorTexture = Content.Load<Texture2D>("Cursor");
    }

    protected override void Update(GameTime gameTime) {
        var kState = Keyboard.GetState();
        var mState = Mouse.GetState();

        if (kState.IsKeyDown(Keys.Escape))
            Exit();

        _tickAccumulator = Math.Min(
            _tickAccumulator + gameTime.ElapsedGameTime.TotalSeconds,
            MaximumAccumulatedTime);

        while (_tickAccumulator >= SimulationTickDuration) {
            if (kState.IsKeyDown(Keys.F3) && _prevKeyboardState.IsKeyUp(Keys.F3)) {
                _showDebug = !_showDebug;
            }

            HandleUniversalInput(kState, mState);
            _prevMouseState = mState;
            _prevKeyboardState = kState;
            _tickAccumulator -= SimulationTickDuration;
        }

        base.Update(gameTime);
    }

    private void HandleUniversalInput(KeyboardState kState, MouseState mState) {
        // Keyboard movement remains available alongside mouse dragging on desktop.
        float cameraSpeed = 8f;
        if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) cameraSpeed *= 2.5f;

        if (kState.IsKeyDown(Keys.A)) WorldCameraX -= cameraSpeed;
        if (kState.IsKeyDown(Keys.D)) WorldCameraX += cameraSpeed;
        if (kState.IsKeyDown(Keys.W)) WorldCameraY -= cameraSpeed;
        if (kState.IsKeyDown(Keys.S)) WorldCameraY += cameraSpeed;

        if (mState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Pressed) {
            Vector2 mouseDelta = mState.Position.ToVector2() - _prevMouseState.Position.ToVector2();
            WorldCameraX -= mouseDelta.X / Zoom;
            WorldCameraY -= mouseDelta.Y / Zoom;
        }

        if (mState.ScrollWheelValue != _prevMouseState.ScrollWheelValue) {
            int scrollDelta = mState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
            _targetZoom += Math.Sign(scrollDelta) * 0.25f;
            _targetZoom = MathHelper.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        float screenCenterX = GraphicsDevice.Viewport.Width / 2f;
        float screenCenterY = GraphicsDevice.Viewport.Height / 2f;
        var touchState = TouchPanel.GetState();
        bool isPinching = touchState.Count >= 2;

        if (isPinching) {
            Vector2 firstPosition = touchState[0].Position;
            Vector2 secondPosition = touchState[1].Position;
            float pinchDistance = Vector2.Distance(firstPosition, secondPosition);
            Vector2 pinchCenter = (firstPosition + secondPosition) / 2f;

            if (_previousPinchDistance > 0f && pinchDistance > 0f) {
                _targetZoom = MathHelper.Clamp(
                    _targetZoom * pinchDistance / _previousPinchDistance,
                    _minZoom,
                    _maxZoom);
            }

            _previousPinchDistance = pinchDistance;
            _previousTouchId = -1;

            float previousZoom = Zoom;
            Zoom = MathHelper.Lerp(Zoom, _targetZoom, 0.15f);
            Vector2 centerOffset = pinchCenter - new Vector2(screenCenterX, screenCenterY);
            WorldCameraX += centerOffset.X * (1f / previousZoom - 1f / Zoom);
            WorldCameraY += centerOffset.Y * (1f / previousZoom - 1f / Zoom);
            _mouseScreenPos = pinchCenter;
        } else {
            _previousPinchDistance = 0f;
            if (touchState.Count == 1) {
                var touch = touchState[0];
                _mouseScreenPos = touch.Position;

                if (touch.Id == _previousTouchId && touch.State == TouchLocationState.Moved) {
                    Vector2 touchDelta = touch.Position - _previousTouchPosition;
                    WorldCameraX -= touchDelta.X / Zoom;
                    WorldCameraY -= touchDelta.Y / Zoom;
                }

                _previousTouchId = touch.Id;
                _previousTouchPosition = touch.Position;
            } else {
                _previousTouchId = -1;
                _mouseScreenPos = mState.Position.ToVector2();
            }

            Zoom = MathHelper.Lerp(Zoom, _targetZoom, 0.15f);
        }

        float maxMapX = MapWidth * TileSize;
        float maxMapY = MapHeight * TileSize;
        WorldCameraX = MathHelper.Clamp(WorldCameraX, 0f, maxMapX);
        WorldCameraY = MathHelper.Clamp(WorldCameraY, 0f, maxMapY);

        float mouseWorldX = (_mouseScreenPos.X - screenCenterX) / Zoom + WorldCameraX;
        float mouseWorldY = (_mouseScreenPos.Y - screenCenterY) / Zoom + WorldCameraY;

        int tileX = (int)Math.Floor(mouseWorldX / TileSize);
        int tileY = (int)Math.Floor(mouseWorldY / TileSize);

        if (tileX >= 0 && tileX < MapWidth && tileY >= 0 && tileY < MapHeight) {
            _hoveredTile = new Point(tileX, tileY);
        } else {
            _hoveredTile = new Point(-1, -1);
        }
    }

    protected override void Draw(GameTime gameTime) {
        UpdateRenderedFps();
        GraphicsDevice.Clear(Color.Black);

        if (_spriteBatch == null) {
            base.Draw(gameTime);
            return;
        }

        float screenCenterX = GraphicsDevice.Viewport.Width / 2f;
        float screenCenterY = GraphicsDevice.Viewport.Height / 2f;

        Matrix cameraMatrix = Matrix.CreateTranslation(new Vector3(-WorldCameraX, -WorldCameraY, 0)) *
                             Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                             Matrix.CreateTranslation(new Vector3(screenCenterX, screenCenterY, 0));

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, cameraMatrix);

        float halfViewWidth = screenCenterX / Zoom;
        float halfViewHeight = screenCenterY / Zoom;
        int minTileX = Math.Max(0, (int)Math.Floor((WorldCameraX - halfViewWidth) / TileSize));
        int maxTileX = Math.Min(MapWidth - 1, (int)Math.Floor((WorldCameraX + halfViewWidth) / TileSize));
        int minTileY = Math.Max(0, (int)Math.Floor((WorldCameraY - halfViewHeight) / TileSize));
        int maxTileY = Math.Min(MapHeight - 1, (int)Math.Floor((WorldCameraY + halfViewHeight) / TileSize));

        for (int i = minTileX; i <= maxTileX; i++) {
            for (int j = minTileY; j <= maxTileY; j++) {
                int groundType = GLayer[i, j, 0];
                int textureVariant = GLayer[i, j, 1];
                int currentAngleDegrees = GLayer[i, j, 2];
                Texture2D? texture = groundType == (int)Grounds.UMad
                    ? _specialTexture
                    : textureVariant >= 1 && textureVariant < _grassTextures.Length
                        ? _grassTextures[textureVariant]
                        : null;

                if (texture != null) {
                    Vector2 tilePos = new Vector2(i * TileSize, j * TileSize);
                    Color tileColor = (i == _hoveredTile.X && j == _hoveredTile.Y) ? new Color(255, 255, 200) : Color.White;

                    if (groundType == (int)Grounds.Grass) {
                        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                        Vector2 tileCenterPosition = tilePos + new Vector2(TileSize / 2f, TileSize / 2f);
                        float radians = MathHelper.ToRadians(currentAngleDegrees);

                        _spriteBatch.Draw(texture, tileCenterPosition, null, tileColor, radians, origin, 1f, SpriteEffects.None, 0f);
                    } else {
                        _spriteBatch.Draw(texture, tilePos, tileColor);
                    }
                }
            }
        }

        _spriteBatch.End();

        // Interface
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

        const int fpsBoxWidth = 90;
        int fpsBoxX = GraphicsDevice.Viewport.Width - fpsBoxWidth - 12;
        _spriteBatch.Draw(_pixelBackground!, new Rectangle(fpsBoxX, 12, fpsBoxWidth, 32), Color.Black * 0.65f);
        if (_debugFont != null) {
            _spriteBatch.DrawString(_debugFont, _fpsText, new Vector2(fpsBoxX + 8, 18), Color.LimeGreen);
        }

        if (_showDebug && _debugFont != null && _pixelBackground != null) {
            string camText = $"Camera Pos: X:{Math.Round(WorldCameraX)}, Y:{Math.Round(WorldCameraY)} | Zoom: {Math.Round(Zoom, 2)}";
            string mouseText = $"Cursor Tile: X:{_hoveredTile.X}, Y:{_hoveredTile.Y}";
            string fullDebugInfo = $"{camText}\n{mouseText}";
            _spriteBatch.Draw(_pixelBackground, new Rectangle(10, 10, 460, 75), Color.Black * 0.6f);
            _spriteBatch.DrawString(_debugFont, fullDebugInfo, new Vector2(20, 15), Color.LimeGreen);
        }
        #if !ANDROID
        if (_cursorTexture != null) {
            _spriteBatch.Draw(_cursorTexture, _mouseScreenPos, Color.White);
        }
        #endif

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void UpdateRenderedFps() {
        _renderedFrames++;
        long now = Stopwatch.GetTimestamp();
        long elapsedTicks = now - _fpsWindowStartTimestamp;

        if (elapsedTicks >= Stopwatch.Frequency) {
            _fps = _renderedFrames * (double)Stopwatch.Frequency / elapsedTicks;
            _fpsText = $"FPS: {Math.Round(_fps)}";
            _renderedFrames = 0;
            _fpsWindowStartTimestamp = now;
        }
    }
}