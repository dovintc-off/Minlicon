// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://www.gnu.org/licenses/gpl-3.0.txt
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class FontRegistry {
    private static readonly object SyncRoot = new();
    private static readonly IReadOnlyDictionary<Font, string> FontResources = new Dictionary<Font, string> {
        [Font.Medeo] = "Minlicon.Fonts.MedeoSans-Black.ttf"
    };
    private static readonly Dictionary<Font, FontSystem> FontSystems = new();
    private static readonly Dictionary<(Font Font, int Size), SpriteFontBase> FontCache = new();
    private static bool _initialized;

    public static void Initialize() {
        lock (SyncRoot) {
            if (_initialized)
                return;

            var loadedSystems = new Dictionary<Font, FontSystem>();
            foreach (var entry in FontResources) {
                using Stream? fontStream = typeof(FontRegistry).Assembly.GetManifestResourceStream(entry.Value);
                if (fontStream == null)
                    throw new FileNotFoundException($"В сборке не найден ресурс шрифта: {entry.Value}");

                using var buffer = new MemoryStream();
                fontStream.CopyTo(buffer);
                var fontSystem = new FontSystem();
                fontSystem.AddFont(buffer.ToArray());
                loadedSystems.Add(entry.Key, fontSystem);
            }

            foreach (var entry in loadedSystems)
                FontSystems.Add(entry.Key, entry.Value);
            _initialized = true;
        }
    }

    public static SpriteFontBase Get(Font font, int size) {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Размер слишком маленький");

        lock (SyncRoot) {
            if (!_initialized)
                throw new InvalidOperationException("Менеджер шрифтов еще не инициализирован");
            if (!FontSystems.TryGetValue(font, out FontSystem? fontSystem))
                throw new ArgumentOutOfRangeException(nameof(font), font, "Шрифт не существует");

            var key = (font, size);
            if (FontCache.TryGetValue(key, out SpriteFontBase? cachedFont))
                return cachedFont;

            SpriteFontBase spriteFont = fontSystem.GetFont(size);
            FontCache.Add(key, spriteFont);
            return spriteFont;
        }
    }
}

public static class SpriteBatchExtensions {
    public static void DrawText(this SpriteBatch spriteBatch, Font font, int size, string text, Vector2 position, Color color) {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        ArgumentNullException.ThrowIfNull(text);
        FontRegistry.Get(font, size).DrawText(spriteBatch, text, position, color);
    }
}

public enum Font {
    Medeo
}