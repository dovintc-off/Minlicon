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
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch? _spriteBatch;

    protected const int MapWidth = 100;
    protected const int MapHeight = 100;
    protected const int TileSize = 32;

    protected readonly int[,,] GLayer = new int[MapWidth, MapHeight, 3];
    protected int[,] BLayer = new int[MapWidth, MapHeight];
    protected readonly Texture2D?[] _grassTextures = new Texture2D?[5];
    protected Texture2D? _specialTexture;

    protected float WorldCameraX = 0f;
    protected float WorldCameraY = 0f;

    protected float Zoom = 1.0f;
    protected float _targetZoom = 1.0f;
    protected float _minZoom = 0.5f;
    protected float _maxZoom = 1.5f;

    protected Texture2D? _cursorTexture;
    protected Point _hoveredTile = new Point(-1, -1);
    protected Vector2 _mouseScreenPos;

    protected SpriteFont? _debugFont;
    protected bool _showDebug = false;
    protected double _fps;
    protected double _tickAccumulator;
    protected long _fpsWindowStartTimestamp;
    protected int _renderedFrames;
    protected string _fpsText = "FPS: 0";
    protected Texture2D? _pixelBackground;

    protected const double SimulationTickDuration = 1.0 / 60.0;
    protected const double MaximumAccumulatedTime = 0.25;

    protected MouseState _prevMouseState;
    protected KeyboardState _prevKeyboardState;
    protected int _previousTouchId = -1;
    protected  Vector2 _previousTouchPosition;
    protected  float _previousPinchDistance;

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

    // Dynamic adaptation
    protected virtual void ConfigurePlatformGraphics() {}
    // Input
    protected virtual void HandlePlatformInput(KeyboardState kState, MouseState mState) {}
    // Cursor
    protected virtual void DrawPlatformCursor(SpriteBatch spriteBatch, Texture2D cursorTexture) {}

    protected override void Initialize() {
        
        ConfigurePlatformGraphics();
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
        FontRegistry.Initialize();

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

            HandlePlatformInput(kState, mState);
            _prevMouseState = mState;
            _prevKeyboardState = kState;
            _tickAccumulator -= SimulationTickDuration;

            _hoveredTile = new Point(
                (int)Math.Floor(((_mouseScreenPos.X - GraphicsDevice.Viewport.Width / 2f) / Zoom + WorldCameraX) / TileSize),
                (int)Math.Floor(((_mouseScreenPos.Y - GraphicsDevice.Viewport.Height / 2f) / Zoom + WorldCameraY) / TileSize)
            );    

            WorldCameraX = MathHelper.Clamp(WorldCameraX, 0f, MapWidth * TileSize);
            WorldCameraY = MathHelper.Clamp(WorldCameraY, 0f, MapHeight * TileSize);
        }

        base.Update(gameTime);
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

        int fpsBoxX = GraphicsDevice.Viewport.Width - (20 + (int)FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X);

        _spriteBatch.Draw(_pixelBackground!, 
            // x y w h 
            new Rectangle(fpsBoxX, 12, 
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X) + 12,
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).Y) + 12
            ), Color.Black * 0.65f);

        _spriteBatch.DrawText(Font.Medeo, 28, _fpsText, new Vector2(fpsBoxX + 6, 18), Color.LimeGreen);

        _spriteBatch.DrawText(Font.Medeo, 30, FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X.ToString(), new Vector2(0, 0), Color.Black);
        
        if (_showDebug && _pixelBackground != null) {
            string camText = $"Camera Pos: X:{Math.Round(WorldCameraX)}, Y:{Math.Round(WorldCameraY)} | Zoom: {Math.Round(Zoom, 2)}";
            string mouseText = $"Cursor Tile: X:{_hoveredTile.X}, Y:{_hoveredTile.Y}";
            string fullDebugInfo = $"{camText}\n{mouseText}";
            _spriteBatch.Draw(_pixelBackground, new Rectangle(10, 10, 460, 75), Color.Black * 0.6f);
            _spriteBatch.DrawText(Font.Medeo, 28, fullDebugInfo, new Vector2(20, 15), Color.LimeGreen);
        }

        DrawPlatformCursor(_spriteBatch, _cursorTexture);

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