using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.Graphics;

namespace Minlicon.Core;

public partial class Main {
    protected bool _showDebug = false;
    protected double _fps;
    protected long _fpsWindowStartTimestamp;
    protected int _renderedFrames;
    protected string _fpsText = "FPS: 0";
    protected Texture2D? _pixelBackground;

    protected virtual void DrawPlatformCursor(SpriteBatch spriteBatch, Texture2D cursorTexture) {}

    protected override void Draw(GameTime gameTime) {
        UpdateRenderedFps();
        GraphicsDevice.Clear(Color.Black);

        if (_spriteBatch == null || _worldDrawer == null) {
            base.Draw(gameTime);
            return;
        }

        float screenCenterX = GraphicsDevice.Viewport.Width / 2f;
        float screenCenterY = GraphicsDevice.Viewport.Height / 2f;

        Matrix cameraMatrix = Matrix.CreateTranslation(new Vector3(-WorldCameraX, -WorldCameraY, 0)) *
                             Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                             Matrix.CreateTranslation(new Vector3(screenCenterX, screenCenterY, 0));

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, cameraMatrix);
        _worldDrawer.Draw(WorldCameraX, WorldCameraY, Zoom, GraphicsDevice.Viewport, _hoveredTile);
        _spriteBatch.End();

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

        DrawDebugUI();
        DrawPlatformCursor(_spriteBatch, _cursorTexture!);

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

    private void DrawDebugUI() {
        int fpsBoxX = GraphicsDevice.Viewport.Width - (20 + (int)FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X);

        _spriteBatch!.Draw(_pixelBackground!, 
            new Rectangle(fpsBoxX, 12, 
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X) + 12,
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).Y) + 12
            ), Color.Black * 0.65f);

        _spriteBatch.DrawText(Font.Medeo, 28, _fpsText, new Vector2(fpsBoxX + 6, 18), Color.LimeGreen);
        
        if (_showDebug && _pixelBackground != null) {
            string camText = $"Camera Pos: X:{Math.Round(WorldCameraX)}, Y:{Math.Round(WorldCameraY)} | Zoom: {Math.Round(Zoom, 2)}";
            string mouseText = $"Cursor Tile: X:{_hoveredTile.X}, Y:{_hoveredTile.Y}";
            string fullDebugInfo = $"{camText}\n{mouseText}";
            
            _spriteBatch.Draw(_pixelBackground, new Rectangle(10, 10, 460, 75), Color.Black * 0.6f);
            _spriteBatch.DrawText(Font.Medeo, 28, fullDebugInfo, new Vector2(20, 15), Color.LimeGreen);
        }
    }
}
