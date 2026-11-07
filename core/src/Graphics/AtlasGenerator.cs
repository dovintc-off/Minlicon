using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.TileSystem;

namespace Minlicon.Core.Graphics;

public class AtlasGenerator {
    private List<string> MGCBTextures = new List<string>();
    private ContentManager contentManager;
    private GraphicsDevice graphicsDevice;
    private AtlasType Atlas;

    public class AtlasType {
        public Texture2D? AtlasTexture { get; set; }
        public Dictionary<string, Rectangle> Regions { get; set; } = new Dictionary<string, Rectangle>();
    }

    public AtlasType GetAtlas() {
        return this.Atlas;
    }

    public AtlasGenerator(ContentManager _contentManager, GraphicsDevice _graphicsDevice) {
        if (_contentManager == null || _graphicsDevice == null) throw new Exception("ContentManager or GraphicsDevice is null");
        contentManager = _contentManager;
        graphicsDevice = _graphicsDevice;
        InitMGCBTextues();
        Atlas = Generate();
    }

    private void InitMGCBTextues() {
        using (Stream stream = TitleContainer.OpenStream("Content/index.txt"))
        using (StreamReader reader = new StreamReader(stream)) {
            foreach (string line in reader.ReadToEnd().Split(new char[] { '\n', '\r' })) {
                if (!string.IsNullOrWhiteSpace(line))
                    MGCBTextures.Add(line);
            }
        }
    }

    private AtlasType Generate() {
        var _atlas = new AtlasType();
        var loadedTextures = new List<(string Name, Texture2D Texture)>();
        int totalWidth = 0;
        int maxHeight = 0;

        foreach (var _path in MGCBTextures) {
            if (string.IsNullOrWhiteSpace(_path)) continue;
            string path = _path.Replace('\\', '/');

            try {
                Texture2D _texture2D = contentManager.Load<Texture2D>(path);
                loadedTextures.Add((path, _texture2D));

                totalWidth += _texture2D.Width;
                maxHeight = Math.Max(maxHeight, _texture2D.Height);
            } catch (Exception e) {
                Console.WriteLine($"Cant load texture {e}");
            }
        }

        if (loadedTextures.Count == 0) return _atlas;

        RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, totalWidth, maxHeight);

        graphicsDevice.SetRenderTarget(renderTarget);
        graphicsDevice.Clear(Color.Transparent);

        SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
        spriteBatch.Begin();

        int currentX = 0;
        foreach (var item in loadedTextures) {
            spriteBatch.Draw(item.Texture, new Vector2(currentX, 0), Color.White);
            _atlas.Regions[item.Name] = new Rectangle(currentX, 0, item.Texture.Width, item.Texture.Height);
            currentX += item.Texture.Width;
        }

        spriteBatch.End();
        graphicsDevice.SetRenderTarget(null);
        _atlas.AtlasTexture = renderTarget;

        return _atlas;
    }

    public void SaveAtlas(string path) {
        if (Atlas?.AtlasTexture == null)
            throw new InvalidOperationException("Atlas texture is null.");

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using FileStream stream = File.Create(path);
        Atlas.AtlasTexture.SaveAsPng(stream, Atlas.AtlasTexture.Width, Atlas.AtlasTexture.Height);
    }

    public void BakeRegistry(TileRegistry registry) {
        if (this.Atlas == null || this.Atlas.Regions == null) return;

        Rectangle umadRect = Rectangle.Empty;
        foreach (var key in this.Atlas.Regions.Keys) {
            if (key.EndsWith("UMad", StringComparison.OrdinalIgnoreCase)) {
                umadRect = this.Atlas.Regions[key];
                break;
            }
        }

        foreach (var building in registry.Buildings.Values) {
            if (building.Drawer?.Layers != null && building.Drawer.Layers.Count > 0) {
                foreach (var layer in building.Drawer.Layers) {
                    string safeLayerPath = layer.Image.Replace('\\', '/');
                    
                    if (this.Atlas.Regions.TryGetValue(safeLayerPath, out Rectangle rect)) {
                        layer.AtlasRect = rect; 
                    } else {
                        Console.WriteLine($"[Atlas] Слой '{layer.Image}' для здания '{building.Name}' не найден. Установлена заглушка UMad.");
                        layer.AtlasRect = umadRect;
                    }
                }
            } 
            else {
                string foundKey = null!;
                foreach (var key in this.Atlas.Regions.Keys) {
                    if (key.EndsWith(building.Name, StringComparison.OrdinalIgnoreCase)) {
                        foundKey = key;
                        break;
                    }
                }

                Rectangle finalRect = foundKey != null ? this.Atlas.Regions[foundKey] : umadRect;
                if (foundKey == null) {
                    Console.WriteLine($"[Atlas] Текстура по умолчанию для здания '{building.Name}' не найдена. Установлена заглушка UMad.");
                }

                var defaultLayer = new LayerDefinition {
                    Order = 0,
                    Image = foundKey ?? "UMad",
                    AtlasRect = finalRect
                };
                building.Drawer!.Layers!.Add(defaultLayer);
            }
        }

        foreach (var ground in registry.Grounds.Values) {
            if (ground.RandomDrawer?.Textures != null && ground.RandomDrawer.Textures.Count > 0) {
                var rectList = new List<Rectangle>();
                
                foreach (var texPath in ground.RandomDrawer.Textures) {
                    string safeTexPath = texPath.Replace('\\', '/');
                    
                    if (this.Atlas.Regions.TryGetValue(safeTexPath, out Rectangle rect)) {
                        rectList.Add(rect);
                    } else {
                        Console.WriteLine($"[Atlas] Текстура земли '{texPath}' для '{ground.Name}' не найдена. Установлена заглушка UMad.");
                        rectList.Add(umadRect);
                    }
                }
                ground.Variants = rectList.ToArray();
            } else {
                ground.Variants = new Rectangle[] { umadRect };
            }
        }
    }
}
