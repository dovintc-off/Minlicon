using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace Minlicon.Core.TileSystem;

public sealed class GroundDefinition  {
    public string Type { get; init; } = "Ground";
    public string Name { get; init; } = "";
    public bool CanBuild { get; init; } = true;
    public RandomDrawerDefinition? RandomDrawer { get; init; }

    [JsonIgnore] public int Id { get; set; }
    [JsonIgnore] public string[] VariantFiles { get; set; } = Array.Empty<string>();
    [JsonIgnore] public Rectangle[] Variants { get; set; } = Array.Empty<Rectangle>();
    [JsonIgnore] public int[] RotateSteps { get; set; } = new int[] { 0 }; 
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
    public BuildingDefinition(string name, int width, int height, int maxHealth, bool rotateble = false) {
        Name = name;
        Width = width;
        Height = height;
        MaxHealth = maxHealth;
        Rotateble = rotateble;
    }

    public string Name { get; set; } = "";
    public int MaxHealth { get; set; }
    public string Type { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Rotateble { get; set; }
    public string? OutputPoint { get; set; }
    public DrawerDefinition Drawer { get; init; } = new();
    [JsonIgnore] public int StandOnGroundId { get; set; } = -1;
}

public sealed class RotationDefinition {
    public string Pivot { get; init; } = "TextureCenter";
    public string Direction { get; init; } = "Clockwise";
    public float Speed { get; init; } 
}
