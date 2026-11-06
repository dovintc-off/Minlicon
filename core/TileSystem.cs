namespace Minlicon.Core.TileSystem;

using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using System.Text.Json;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class TileSystem {
    private const int MaxWorldSize = 2000;
    private const int MinWorldSize = 100;
    private readonly TileCell[,] World;

    public TileSystem(int Size, GroundDefinition defaultGround): this(Size, Size, defaultGround){}

    public TileSystem(int width, int height, GroundDefinition defaultGround) {
        if (width < MinWorldSize || height < MinWorldSize ||
            width > MaxWorldSize || height > MaxWorldSize) {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                $"Размер мира должен быть от {MinWorldSize} до {MaxWorldSize}."
            );
        }

        World = new TileCell[width, height];

        Random random = new();

        var rotations = defaultGround.RandomDrawer?.Rotates;
        int[] availableRotations = rotations != null && rotations.Count > 0
            ? rotations.ToArray()
            : new[] { 0 };

        int variantsCount = defaultGround.Variants.Length;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                World[x, y] = new TileCell(defaultGround, null) {
                    Rotate = availableRotations[random.Next(availableRotations.Length)],
                    TextureVariant = variantsCount > 0
                        ? random.Next(variantsCount)
                        : 0
                };
            }
        }
    }

    public TileCell Get(int x, int y) {
        if (IsInBounds(x, y)) {
            return this.World[x, y];
        } else {
            throw new ArgumentOutOfRangeException(nameof(x), "Координаты находятся за пределами карты.");
        }
        
    }

    public void Set(int x, int y, TileCell newTile) {
        if (IsInBounds(x, y)) {
            this.World[x, y] = newTile;
        } else {
            throw new ArgumentOutOfRangeException(nameof(x), "Координаты находятся за пределами карты.");
        }
    }

    private bool IsInBounds(int x, int y){
        return this.World.GetLength(0) > x && this.World.GetLength(1) > y && y >= 0 && x >= 0;
    }

    public bool CanPlace(int x, int y, int width, int height) {
        if (x < 0 || y < 0 || x + width > World.GetLength(0) || y + height > World.GetLength(1)) return false;
        for (int _y = 0; _y < height; _y++) {
            for (int _x = 0; _x < width; _x++) {
                TileCell cell = World[_x + x, _y + y];
                if (cell.Building != null) {
                    return false;
                }
            }
        }
        return true;
    }

    public bool PlaceBuilding(BuildingInstance building){
        var (width, height) = GetRotatedSize(building);
        if (!CanPlace(building.X, building.Y, width, height)){
            return false;
        }
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                World[x + building.X, y + building.Y].Building = building;
            }
        }
        return true;
    }

    private (int Width, int Height) GetRotatedSize(BuildingInstance building) {
        if (building.Rotate == 1 || building.Rotate == 3) {
            return (building.Definition.Height, building.Definition.Width);
        }
        return (building.Definition.Width, building.Definition.Height);
    }


    public bool CanRemove(int x, int y){
        if (x < 0 || x >= World.GetLength(0) || y < 0 || y >= World.GetLength(1) || World[x,y].Building == null) {
            return false;
        }

        BuildingInstance building = World[x,y].Building!;
        int width = building.Definition.Width;
        int height = building.Definition.Height;

        for (int _y = 0; _y<height; _y++) {
            for (int _x = 0; _x<width; _x++) {
                if (building != World[_x+building.X,_y+building.Y].Building) return false; 
            }
        }
        return true;
    }

    public bool RemoveBuilding(int x, int y) {
        if (!CanRemove(x, y)) return false;
        BuildingInstance building = World[x, y].Building!;
        var (width, height) = GetRotatedSize(building);
        for (int _y = 0; _y < height; _y++) {
            for (int _x = 0; _x < width; _x++) {
                World[_x + building.X, _y + building.Y].Building = null;
            }
        }
        return true;
    }
}

public struct TileCell {
    public TileCell(GroundDefinition ground, BuildingInstance? building) {
        Ground = ground;
        Building = building;
        Rotate = 0;
    }
    public GroundDefinition Ground { get; set; }
    public BuildingInstance? Building { get; set; }
    public int Rotate { get; set; }
    public int TextureVariant { get; set; }
}

public class BuildingInstance {
    public BuildingDefinition Definition {get;}

    public int X {get;}
    public int Y {get;}

    private int _rotate;

    public int Rotate {
        get => _rotate;
        set => _rotate = Definition.Rotateble ? ((value % 4) + 4) % 4 : _rotate;
    }

    public int Health {get; set;}

    public BuildingInstance(
        BuildingDefinition definition,
        int x,
        int y
    ) {
        Definition = definition;
        X = x;
        Y = y;
        Health = definition.MaxHealth;
    }
}

public sealed class GroundDefinition {
    public string Type { get; init; } = "Ground";
    public string Name { get; init; } = "";
    public bool CanBuild { get; init; } = true;
    public RandomDrawerDefinition? RandomDrawer { get; init; }

    [JsonIgnore] public int Id { get; set; }
    [JsonIgnore] public string[] VariantFiles { get; set; } = Array.Empty<string>();
    [JsonIgnore] public Rectangle[] Variants { get; set; } = Array.Empty<Rectangle>();
    [JsonIgnore] public int[] RotateSteps { get; set; } = { 0 };   // 0..3
}

public sealed class RandomDrawerDefinition {
    public List<string> Textures { get; init; } = new();
    public List<int> Rotates { get; init; } = new() { 0 };
}

public sealed class DrawerDefinition {
    public List<LayerDefinition> Layers { get; init; } = new();
}

public sealed class LayerDefinition {
    public int Order { get; init; }
    public string Image { get; init; } = "";
    public RotationDefinition? Rotation { get; init; }

    [JsonIgnore] public Rectangle AtlasRect { get; set; }
}

public class BuildingDefinition {
    public BuildingDefinition(string name, int width, int height, int maxHealth, bool rotateble = false){
        Name = name;
        Width = width;
        Height = height;
        MaxHealth = maxHealth;
        Rotateble = rotateble;
    }

    public string Name {get; set;} = "";
    public int MaxHealth {get; set;}
    public string Type {get; set;} = "";
    public int Width {get; set;}
    public int Height {get; set;}
    public bool Rotateble {get; set;}
    public string? OutputPoint {get; set;}
    public DrawerDefinition Drawer { get; init; } = new();
    [JsonIgnore] public int StandOnGroundId { get; set; } = -1;
}

public sealed class RotationDefinition {
    public string Pivot { get; init; } = "TextureCenter";
    public string Direction { get; init; } = "Clockwise";
    public float Speed { get; init; } //　degers per second 　
}

public class TileRegistry {
    public Dictionary<string, GroundDefinition> Grounds { get; } = new();
    public Dictionary<string, BuildingDefinition> Buildings { get; } = new();
    // Here "path to file (full)": "content of file"
    private Dictionary<string, string> JsonData = new Dictionary<string, string>{{"Error", "Umad"}};
    
    public TileRegistry(Dictionary<string, string> JsonData){
        SetJsonData(JsonData);
        Load();
    }

    private void SetJsonData(Dictionary<string, string> data) {
        foreach (var (key, value) in data) {
            if (key == null || value == null) continue;
            this.JsonData[key] = value;   
        }
    }

    private void Load() {    
        foreach (var (key, value) in JsonData) {
            if (key == "Error") continue;
            try {
                using (JsonDocument doc = JsonDocument.Parse(value)) {
                    if (doc.RootElement.TryGetProperty("Type", out JsonElement typeElement)) {
                        string? type = typeElement.GetString();

                        if (type != null && type == "Building") {
                            // case-insensitive
                            var building = JsonSerializer.Deserialize<BuildingDefinition>(value, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

                            if (building != null) {
                                this.Buildings[building.Name] = building;
                            } else {
                                Console.WriteLine($"Wrong structure for {key}");
                            }
                        }

                        else if (type != null && type == "Ground") {
                            // case-insensitive
                            var ground = JsonSerializer.Deserialize<GroundDefinition>(value, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

                            if (ground != null) {
                                this.Grounds[ground.Name] = ground;
                            } else {
                                Console.WriteLine($"Wrong structure for {key}");
                            }
                        }
                        else {
                            Console.WriteLine($"unknown type: {type} from {key}");
                        }
                    }
                }
            } catch (Exception error) {
                Console.WriteLine($"U Mad?\nError: {error}");
            }
        }
    }

    public void BindAtlasRegions(AtlasGenerator.AtlasType atlas) {
        if (atlas == null || atlas.Regions == null) return;

        foreach (var building in this.Buildings.Values) {
            if (building.Drawer != null && building.Drawer.Layers != null && building.Drawer.Layers.Count > 0) {
                foreach (var layer in building.Drawer.Layers) {
                    string safeLayerPath = layer.Image.Replace('\\', '/');

                    if (atlas.Regions.TryGetValue(safeLayerPath, out Rectangle rect)) {
                        layer.AtlasRect = rect;
                    } else {
                        Console.WriteLine($"[Warning] Слой '{layer.Image}' для здания '{building.Name}' не найден на атласе.");
                    }
                }
            } 
            else {
                string foundKey = null!;
                foreach (var key in atlas.Regions.Keys) {
                    if (key.EndsWith(building.Name, StringComparison.OrdinalIgnoreCase)) {
                        foundKey = key;
                        break;
                    }
                }

                if (foundKey != null) {
                    var defaultLayer = new LayerDefinition {
                        Order = 0,
                        Image = foundKey,
                        AtlasRect = atlas.Regions[foundKey]
                    };
                    building.Drawer!.Layers!.Add(defaultLayer);
                } else {
                    Console.WriteLine($"[Warning] Текстура по умолчанию для здания '{building.Name}' не найдена на атласе.");
                }
            }
        }

        foreach (var ground in this.Grounds.Values) {
            if (ground.RandomDrawer != null && ground.RandomDrawer.Textures != null) {
                var rectList = new List<Rectangle>();

                foreach (var texPath in ground.RandomDrawer.Textures) {
                    string safeTexPath = texPath.Replace('\\', '/');

                    if (atlas.Regions.TryGetValue(safeTexPath, out Rectangle rect)) {
                        rectList.Add(rect);
                    } else {
                        Console.WriteLine($"[Warning] Текстура земли '{texPath}' для '{ground.Name}' не найдена на атласе.");
                    }
                }

                ground.Variants = rectList.ToArray();
            }
        }
    }
}

public class AtlasGenerator {
    private List<string> MGCBTextures = new List<string>();
    private ContentManager contentManager;
    private GraphicsDevice graphicsDevice;
    private AtlasType Atlas;

    public class AtlasType {
        public Texture2D? AtlasTexture { get; set; }
        public Dictionary<string, Rectangle> Regions { get; set; } = new Dictionary<string, Rectangle>();
    }

    public AtlasType GetAtlas(){
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
            foreach (string line in reader.ReadToEnd().Split(new char[]{'\n', '\r'})) {
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

public class WorldDrawer {
    private readonly SpriteBatch _spriteBatch;
    private readonly TileSystem _tileSystem;
    private readonly Texture2D _atlasTexture;
    private readonly TileRegistry _registry;
    private readonly int _tileSize;

    public WorldDrawer(SpriteBatch spriteBatch, TileSystem tileSystem, AtlasGenerator.AtlasType atlas, TileRegistry registry, int tileSize = 32) {
        _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        _tileSystem = tileSystem ?? throw new ArgumentNullException(nameof(tileSystem));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _tileSize = tileSize;
        _atlasTexture = atlas?.AtlasTexture ?? throw new ArgumentException("Atlas texture cannot be null");
    }

    public void Draw(float cameraX, float cameraY, float zoom, Viewport viewport, Point hoveredTile) {
        float screenCenterX = viewport.Width / 2f;
        float screenCenterY = viewport.Height / 2f;

        float halfViewWidth = screenCenterX / zoom;
        float halfViewHeight = screenCenterY / zoom;

        int minTileX = Math.Max(0, (int)Math.Floor((cameraX - halfViewWidth) / _tileSize));
        int maxTileX = Math.Min(99, (int)Math.Floor((cameraX + halfViewWidth) / _tileSize));
        int minTileY = Math.Max(0, (int)Math.Floor((cameraY - halfViewHeight) / _tileSize));
        int maxTileY = Math.Min(99, (int)Math.Floor((cameraY + halfViewHeight) / _tileSize));

        for (int x = minTileX; x <= maxTileX; x++) {
            for (int y = minTileY; y <= maxTileY; y++) {
                TileCell cell = _tileSystem.Get(x, y);
                Vector2 tilePos = new Vector2(x * _tileSize, y * _tileSize);
                
                Color tileColor = (x == hoveredTile.X && y == hoveredTile.Y) ? new Color(255, 255, 200) : Color.White;
                GroundDefinition groundDef = cell.Ground;

                if (groundDef.Variants != null && groundDef.Variants.Length > 0) {
                    int variantIndex = Math.Clamp(
                        cell.TextureVariant,
                        0,
                        groundDef.Variants.Length - 1
                    );

                    Rectangle srcRect = groundDef.Variants[variantIndex];

                    Vector2 origin = new Vector2(
                        srcRect.Width / 2f, srcRect.Height / 2f );

                    Vector2 tileCenterPosition = tilePos + new Vector2(
                        _tileSize / 2f, _tileSize / 2f );

                    float radians = MathHelper.ToRadians(cell.Rotate);

                    _spriteBatch.Draw( _atlasTexture, tileCenterPosition, srcRect, tileColor, radians, origin, 1f, SpriteEffects.None, 0f );
                }

                if (cell.Building != null && cell.Building.X == x && cell.Building.Y == y) {
                    DrawBuildingInstance(cell.Building, tileColor);
                }
            }
        }
    }

    private void DrawBuildingInstance(BuildingInstance building, Color color) {
        if (building.Definition.Drawer?.Layers == null || building.Definition.Drawer.Layers.Count == 0) return;

        Vector2 baseTilePos = new Vector2(building.X * _tileSize, building.Y * _tileSize);
        Vector2 origin = new Vector2(_tileSize / 2f, _tileSize / 2f);
        Vector2 centerPosition = baseTilePos + origin;

        float radians = MathHelper.ToRadians(building.Rotate * 90f);

        foreach (var layer in building.Definition.Drawer.Layers) {
            if (layer.AtlasRect == Rectangle.Empty) continue;

            _spriteBatch.Draw( _atlasTexture, centerPosition, layer.AtlasRect, color, radians, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}