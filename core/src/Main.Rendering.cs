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

        if (_spriteBatch == null) {
            base.Draw(gameTime);
            return;
        }

        _sceneManager.Draw(gameTime);

        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp
        );

        DrawDebugUI();

        if (_sceneManager.ActualScene is PlayOnMap scene)
            DrawPlatformCursor(_spriteBatch, scene);

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
        int fpsBoxX = GraphicsDevice.Viewport.Width -
            (20 + (int)FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X);

        _spriteBatch!.Draw(
            _pixelBackground!,
            new Rectangle(fpsBoxX, 12,
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).X) + 12,
                (int)Math.Ceiling(FontRegistry.Get(Font.Medeo, 28).MeasureString(_fpsText).Y) + 12
            ),
            Color.Black * 0.65f
        );

        _spriteBatch.DrawText(Font.Medeo, 28, _fpsText,
            new Vector2(fpsBoxX + 6, 18), Color.LimeGreen
        );

        if (_showDebug && _pixelBackground != null &&
            _sceneManager.ActualScene is PlayOnMap scene) {

            string camText = $"Camera Pos: X:{Math.Round(scene.CameraX)}, Y:{Math.Round(scene.CameraY)} | Zoom: {Math.Round(scene.Zoom, 2)}";
            string mouseText = $"Cursor Tile: X:{scene.HoveredTile.X}, Y:{scene.HoveredTile.Y}";
            string fullDebugInfo = $"{camText}\n{mouseText}";

            _spriteBatch.Draw(_pixelBackground, new Rectangle(10, 10, 460, 75), Color.Black * 0.6f);
            _spriteBatch.DrawText(Font.Medeo, 28, fullDebugInfo, new Vector2(20, 15), Color.LimeGreen);
        }
    }
}
