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
}
