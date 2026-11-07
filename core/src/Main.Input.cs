using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Minlicon.Core;

public partial class Main {
    protected float Zoom = 1.0f;
    protected float _targetZoom = 1.0f;
    protected float _minZoom = 0.5f;
    protected float _maxZoom = 1.5f;

    protected MouseState _prevMouseState;
    protected KeyboardState _prevKeyboardState;
    protected int _previousTouchId = -1;
    protected Vector2 _previousTouchPosition;
    protected float _previousPinchDistance;

    protected virtual void ConfigurePlatformGraphics() {}
    protected virtual void HandlePlatformInput(KeyboardState kState, MouseState mState) {}
    protected virtual Dictionary<string, string> LoadRegistryData() { return new Dictionary<string, string>(); }

    protected void InitializeInput() {
        TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Pinch;
        _prevMouseState = Mouse.GetState();
        _prevKeyboardState = Keyboard.GetState();
    }

    protected void UpdateInput(GameTime gameTime) {
        var kState = Keyboard.GetState();
        var mState = Mouse.GetState();

        HandlePlatformInput(kState, mState);

        if (kState.IsKeyDown(Keys.Escape))
            Exit();

        // Плавный расчет зума на основе дельты времени (DeltaTime)
        float lerpSpeed = 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Zoom = MathHelper.Lerp(Zoom, _targetZoom, MathHelper.Clamp(lerpSpeed, 0f, 1f));

        _tickAccumulator = Math.Min(
            _tickAccumulator + gameTime.ElapsedGameTime.TotalSeconds,
            MaximumAccumulatedTime);

        while (_tickAccumulator >= SimulationTickDuration) {
            if (kState.IsKeyDown(Keys.F3) && _prevKeyboardState.IsKeyUp(Keys.F3)) {
                _showDebug = !_showDebug;
            }

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
    }
}
