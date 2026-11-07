namespace Minlicon.Core.TileSystem;

public class BuildingInstance 
{
    public BuildingDefinition Definition { get; }
    public int X { get; }
    public int Y { get; }

    private int _rotate;
    public int Rotate {
        get => _rotate;
        set => _rotate = Definition.Rotateble ? ((value % 4) + 4) % 4 : _rotate;
    }

    public int Health { get; set; }

    public BuildingInstance(BuildingDefinition definition, int x, int y) {
        Definition = definition;
        X = x;
        Y = y;
        Health = definition.MaxHealth;
    }
}
