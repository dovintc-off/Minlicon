namespace Minlicon.Android;

using Minlicon.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.IO;
using System.Collections.Generic;
using global::Android.Content.Res;

public class MainAndroid : Main {
    public MainAndroid() : base() {}

    protected override void ConfigurePlatformGraphics() {
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
    }

    protected override void HandlePlatformInput(KeyboardState kState, MouseState mState) {
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
            }

            Zoom = MathHelper.Lerp(Zoom, _targetZoom, 0.15f);
        }
    }

    protected override Dictionary<string, string> LoadRegistryData()  {
        var result = new Dictionary<string, string>();
        global::Android.Content.Res.AssetManager assets = Game.Activity.Assets;
        ListAssetsRecursively(assets, "data", result);
        return result;
    }

    private void ListAssetsRecursively(AssetManager assets, string currentPath, Dictionary<string, string> result)  {
        try  {
            string[] items = assets.List(currentPath);
            if (items == null || items.Length == 0) return;
            foreach (string item in items)  {
                string fullPath = string.IsNullOrEmpty(currentPath) ? item : $"{currentPath}/{item}";
                string[] subItems = assets.List(fullPath);
                if (subItems != null && subItems.Length > 0) {
                    ListAssetsRecursively(assets, fullPath, result);
                } else {
                    if (fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) {
                        using (Stream stream = assets.Open(fullPath))
                        using (StreamReader reader = new StreamReader(stream)) 
                        {
                            string jsonContent = reader.ReadToEnd();
                            string key = Path.GetFileNameWithoutExtension(fullPath);
                            result[key] = jsonContent;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Не удалось прочитать ассеты по пути {currentPath}: {ex.Message}");
        }
    }

    protected override void DrawPlatformCursor(SpriteBatch spriteBatch, Texture2D cursorTexture) {}
}
