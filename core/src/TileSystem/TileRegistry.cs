using System.Text.Json;
using Microsoft.Xna.Framework;
using Minlicon.Core.Graphics;

namespace Minlicon.Core.TileSystem;

public class TileRegistry {
    public Dictionary<string, GroundDefinition> Grounds { get; } = new();
    public Dictionary<string, BuildingDefinition> Buildings { get; } = new();
    private Dictionary<string, string> JsonData = new Dictionary<string, string> { { "Error", "Umad" } };
    
    public TileRegistry(Dictionary<string, string> jsonData) {
        SetJsonData(jsonData);
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
                            var building = JsonSerializer.Deserialize<BuildingDefinition>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (building != null) {
                                this.Buildings[building.Name] = building;
                            } else {
                                Console.WriteLine($"Wrong structure for {key}");
                            }
                        }
                        else if (type != null && type == "Ground") {
                            var ground = JsonSerializer.Deserialize<GroundDefinition>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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