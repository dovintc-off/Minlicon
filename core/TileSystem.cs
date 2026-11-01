namespace Minlicon.Core.TileSystem;

public class TileSystem {
    private const int MaxWorldSize = 2000;
    private const int MinWorldSize = 100;
    private readonly TileCell[,] World;

    public TileSystem(int Size): this(Size, Size){}

    public TileSystem(int Width, int Height) {
        if (Width <= MaxWorldSize && Width >= MinWorldSize && Height <= MaxWorldSize && Height >= MinWorldSize) {
            this.World = new TileCell[Width, Height];
        } else {
            throw new ArgumentOutOfRangeException(nameof(Width));
        }

        for (int w = 0; w < Width; w++) {
            for (int h = 0; h < Height; h++) {
                this.World[w, h] = new TileCell(TileGround.Grass, null);
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
        if (x<0||y<0||x+width>World.GetLength(0)||y+height>World.GetLength(1)) return false;
        for (int _y=0;_y<height;_y++) {
            for (int _x=0;_x<width;_x++) {
                TileCell cell = World[_x+x,_y+y];
                if (cell.Building != null) {
                    return false;
                }
            }
        }
        return true;
    }

    public bool PlaceBuilding(BuildingInstance building){
        int width = building.Definition.Width;
        int height = building.Definition.Height;

        if (!CanPlace(building.X, building.Y, width, height)){
            return false;
        }

        for (int y=0;y<height;y++) {
            for (int x=0;x<width;x++) {
                World[x+building.X,y+building.Y].Building = building;
            }
        }

        return true;
    }

    public bool CanRemove(int x, int y){
        if (x < 0 || x >= World.GetLength(0) || y < 0 || y >= World.GetLength(1) || World[x,y].Building == null) {
            return false;
        }

        BuildingInstance building = World[x,y].Building;
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
        
        BuildingInstance building = World[x,y].Building;
        int width = building.Definition.Width;
        int height = building.Definition.Height;
        
        for (int _y = 0; _y<height; _y++) {
            for (int _x = 0; _x<width; _x++) {
                World[_x+building.X,_y+building.Y].Building = null;
            }
        }

        return true;
    }
}

public class TileCell {
    public TileCell(TileGround titleGroundType, BuildingInstance? tileBuildingType) {
        this.TileGroundType = titleGroundType;
        this.Building = tileBuildingType;
    }

    public TileGround TileGroundType {get; set;}
    public BuildingInstance? Building {get; set;}
}

public enum TileGround {
    Umad = -1, // Заглушка / для ошибок загрузки
    Grass = 0
}

public class BuildingInstance {
    public BuildingDefinition Definition {get;}

    public int X {get;}
    public int Y {get;}

    private int _rotate;

    public int Rotate {
        get => _rotate;
        set {
            if (Definition.Rotateble) {
                _rotate = ((value % 4) + 4) % 4;
            }
        }
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

public class GroundDefinition {
    public string Name {get; set;} = "";
    public bool RandomRotate {get; set;}
    public bool CanBuild {get; set;}
    public Animation? Animation {get; set;}
}

public class Animation {
    // ищем кадры "..._1.png, ..._2.png" и рисуем поэтапно
    // Анимация ради жидкостей и чегото еще
    public int FrameCount; // more than 1
    public int FPS; // Animation Frame Per Second
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
    public int Width {get; set;}
    public int Height {get; set;}
    public bool Rotateble {get; set;}
}