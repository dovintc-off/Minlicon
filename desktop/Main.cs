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
    protected override void DrawPlatformCursor(SpriteBatch spriteBatch, PlayOnMap scene) {
        if (scene.CursorTexture != null)
            spriteBatch.Draw(scene.CursorTexture, scene.MouseScreenPosition, Color.White);
    }

    protected override Dictionary<string, string> LoadRegistryData() {
        var result = new Dictionary<string, string>();
        string targetFolder = Path.Combine(AppContext.BaseDirectory, "Content", "json");
        if (!Directory.Exists(targetFolder)) {
            Console.WriteLine($"Папка с данными не найдена по пути: {targetFolder}");
            return result;
        } try {
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

    protected override void HandlePlatformInput(PlayOnMap scene, GameTime gameTime, KeyboardState kState, MouseState mState, MouseState previousMouseState ) {
        float baseSpeed = 400f;

        if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) baseSpeed *= 2.5f;
        float cameraSpeed = baseSpeed * (1f / scene.Zoom) * (float)gameTime.ElapsedGameTime.TotalSeconds;

        (float x, float y) = (0f, 0f);
        if (kState.IsKeyDown(Keys.A)) x -= cameraSpeed;
        if (kState.IsKeyDown(Keys.D)) x += cameraSpeed;
        if (kState.IsKeyDown(Keys.W)) y -= cameraSpeed;
        if (kState.IsKeyDown(Keys.S)) y += cameraSpeed;

        scene.MoveCamera(x, y);
        if (mState.ScrollWheelValue != previousMouseState.ScrollWheelValue) { 
            int delta = mState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            scene.AdjustTargetZoom(Math.Sign(delta) * 0.15f);
        }

        scene.SetMousePosition(mState.Position.ToVector2());
    }
}
