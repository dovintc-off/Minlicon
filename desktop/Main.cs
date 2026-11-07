namespace Minlicon.Desktop;
using Minlicon.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;

public class MainDesktop : Main {
    public MainDesktop() : base(){}
    protected override void ConfigurePlatformGraphics() {
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        Window.Title = "Minlicon";
    }

    protected override Dictionary<string, string> LoadRegistryData() {
        var result = new Dictionary<string, string>();
        string targetFolder = Path.Combine(AppContext.BaseDirectory, "Content", "json");
        if (!Directory.Exists(targetFolder)) {
            Console.WriteLine($"Папка с данными не найдена по пути: {targetFolder}");
            return result;
        }
        try {
            string[] jsonFiles = Directory.GetFiles(targetFolder, "*.json", SearchOption.AllDirectories);

            foreach (string filePath in jsonFiles) {
                string jsonContent = File.ReadAllText(filePath);
                string key = Path.GetFileNameWithoutExtension(filePath);
                result[key] = jsonContent;
            }
        } catch (Exception ex) {
            Console.WriteLine($"Ошибка при чтении JSON файлов: {ex.Message}");
        }
        return result;
    }
    protected override void HandlePlatformInput(GameTime gameTime, KeyboardState kState, MouseState mState){
        // Базовая скорость (пикселей в секунду)
        float baseSpeed = 400f; 
        if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) baseSpeed *= 2.5f;

        // Чем ближе камера (больше Zoom), тем медленнее она ползет
        float cameraSpeed = baseSpeed * (1f / Zoom) * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (kState.IsKeyDown(Keys.A)) WorldCameraX -= cameraSpeed;
        if (kState.IsKeyDown(Keys.D)) WorldCameraX += cameraSpeed;
        if (kState.IsKeyDown(Keys.W)) WorldCameraY -= cameraSpeed;
        if (kState.IsKeyDown(Keys.S)) WorldCameraY += cameraSpeed;

        if (mState.ScrollWheelValue != _prevMouseState.ScrollWheelValue) {
            int scrollDelta = mState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
            _targetZoom += Math.Sign(scrollDelta) * 0.15f;
            _targetZoom = MathHelper.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        // за плавность теперь отвечает ядро
        _mouseScreenPos = mState.Position.ToVector2();
    }

    protected override void DrawPlatformCursor(SpriteBatch spriteBatch, Texture2D cursorTexture) {
        spriteBatch.Draw(cursorTexture, _mouseScreenPos, Color.White);
    }
}
