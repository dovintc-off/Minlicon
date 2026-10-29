namespace Minlicon.Desktop;
using Minlicon.Core;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MainDesktop : Main {
    public MainDesktop() : base(){}
    protected override void ConfigurePlatformGraphics() {
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        Window.Title = "Minlicon";
    }

    protected override void HandlePlatformInput(KeyboardState kState, MouseState mState){
        float cameraSpeed = 8f;
        if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) cameraSpeed *= 2.5f;

        if (kState.IsKeyDown(Keys.A)) WorldCameraX -= cameraSpeed;
        if (kState.IsKeyDown(Keys.D)) WorldCameraX += cameraSpeed;
        if (kState.IsKeyDown(Keys.W)) WorldCameraY -= cameraSpeed;
        if (kState.IsKeyDown(Keys.S)) WorldCameraY += cameraSpeed;

        if (mState.ScrollWheelValue != _prevMouseState.ScrollWheelValue) {
            int scrollDelta = mState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
            _targetZoom += Math.Sign(scrollDelta) * 0.25f;
            _targetZoom = MathHelper.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        Zoom = MathHelper.Lerp(Zoom, _targetZoom, 0.15f);
        _mouseScreenPos = mState.Position.ToVector2();
    }

    protected override void DrawPlatformCursor(SpriteBatch spriteBatch, Texture2D cursorTexture) {
        spriteBatch.Draw(cursorTexture, _mouseScreenPos, Color.White);
    }
}
