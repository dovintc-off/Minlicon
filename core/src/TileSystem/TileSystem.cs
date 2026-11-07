namespace Minlicon.Core.TileSystem;

public class TileSystem {
    private const int MaxWorldSize = 2000;
    private const int MinWorldSize = 100;
    private readonly TileCell[,] World;

    public TileSystem(int Size, GroundDefinition defaultGround) : this(Size, Size, defaultGround) {}

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
            : new int[] { 0 };

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

    private bool IsInBounds(int x, int y) {
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

    public bool PlaceBuilding(BuildingInstance building) {
        var (width, height) = GetRotatedSize(building);
        if (!CanPlace(building.X, building.Y, width, height)) {
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

    public bool CanRemove(int x, int y) {
        if (x < 0 || x >= World.GetLength(0) || y < 0 || y >= World.GetLength(1) || World[x, y].Building == null) {
            return false;
        }

        BuildingInstance building = World[x, y].Building!;
        int width = building.Definition.Width;
        int height = building.Definition.Height;

        for (int _y = 0; _y < height; _y++) {
            for (int _x = 0; _x < width; _x++) {
                if (building != World[_x + building.X, _y + building.Y].Building) return false;
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
        TextureVariant = 0;
    }
    public GroundDefinition Ground { get; set; }
    public BuildingInstance? Building { get; set; }
    public int Rotate { get; set; }
    public int TextureVariant { get; set; }
}