namespace Minlicon.Core.TileSystem;

public class TileSystem {
    private const int MaxWorldSize = 2000;
    private const int MinWorldSize = 100;
    private readonly TileCell[,] World;
    private readonly List<TileDefinition> TileTypes;

    public TileSystem(int Size): this(Size, Size){}

    public TileSystem(int Width, int Height) {
        if (Width <= MaxWorldSize && Width >= MinWorldSize && Height <= MaxWorldSize && Height >= MinWorldSize) {
            this.World = new TileCell[Width, Height];
        } else {
            System.Console.WriteLine("Неподходящий размер карты");
            throw new Exception("Неподходящий размер карты");
        }

        for (int w = 0; w < Width; w++) {
            for (int h = 0; h < Height; h++) {
                this.World[w, h] = new TileCell(TileDefinition.Umad, 0);
            }
        }
    }

    public TileCell Get(int x, int y) {
        if (IsInBounds(x, y)) {
            return this.World[x, y];
        } else {
            System.Console.WriteLine("Неподходящие координаты карты");
            throw new Exception("Неподходящие координаты карты"); 
        }
        
    }

    public void Set(int x, int y, TileCell newTile) {
        if (IsInBounds(x, y)) {
            this.World[x, y] = newTile;
        } else {
            System.Console.WriteLine("Неподходящие координаты карты");
            throw new ArgumentOutOfRangeException("Неподходящие координаты карты"); 
        }
    }

    private bool IsInBounds(int x, int y){
        return this.World.GetLength(0) > x && this.World.GetLength(1) > y && y >= 0 && x >= 0;
    }
}

public class TileCell {
    public TileCell(TileDefinition TitleType, byte TitleRotate) {
        this.TileType = TitleType;
        this.Rotate = TitleRotate; 
    }

    public TileDefinition TileType {get; set;}
    // Rotate * 90
    public byte Rotate {get; set;}
}

public enum TileDefinition {
    Umad = -1, // Заглушка / для ошибок загрузки
    Grass = 0
}

// public class TileDefinition {
//     int Id {get; private set;} = -1;
//     string Name {get; private set;}
//     bool CanBuild {get; private set;}


// }