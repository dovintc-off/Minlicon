namespace Minlicon.Android;

using Minlicon.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using global::Android.Content.Res;

public class MainAndroid : Main {
    public MainAndroid() : base() {}

    private long _lastBackPressTime;
    private GamePadState _prevGamePadState;

    protected override void ConfigurePlatformGraphics() {
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
    }

    protected override void HandlePlatformInput(GameTime gameTime, KeyboardState kState, MouseState mState) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && 
            _prevGamePadState.Buttons.Back == ButtonState.Released) {
            
            long currentTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            long diff = currentTime - _lastBackPressTime;

            if (diff < 5000) {
                Exit();
            } else {
                _lastBackPressTime = currentTime;

                Game.Activity.RunOnUiThread(() => {
                    global::Android.Widget.Toast.MakeText(
                        Game.Activity, 
                        "Вы точно хотите выйти? (Нажмите еще раз)", 
                        global::Android.Widget.ToastLength.Short
                    ).Show();
                });
            }
        }

        _prevGamePadState = GamePad.GetState(PlayerIndex.One);
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
