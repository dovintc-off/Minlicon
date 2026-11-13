namespace Minlicon.Core.Map;

using System.Buffers.Binary;
using Minlicon.Core.TileSystem;

public class MapControl {
    static public Dictionary<string, int> groundList {get; private set;} = new Dictionary<string, int>(){};
    
    public StructuredMapData? Read(string file) {    
        StructuredMapData mapData = new StructuredMapData();

        if (!File.Exists(file)) throw new FileNotFoundException("Файл не найден", file);

        using var f = File.OpenRead(file);
        if (f.Length < 4) throw new Exception();

        using var reader = new BinaryReader(f);
        string signature = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));

        if (signature != "MNLC") throw new Exception();

        groundList.Clear();
        while (f.Position < f.Length) {
            byte BlockType = reader.ReadByte();
            if (f.Position >= f.Length) break;

            if (BlockType == 5) {
                if (f.Position + 4 > f.Length) break;
                uint mapDataLength = BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));
                if (f.Position + mapDataLength > f.Length) break;

                byte[] sequentialWorld = reader.ReadBytes((int)mapDataLength);
                mapData.Grounds = sequentialWorld;
                continue;
            }

            byte dataLength = reader.ReadByte();
            if (f.Position + dataLength > f.Length) break;

            ReadOnlySpan<byte> span = reader.ReadBytes(dataLength);

            switch (BlockType) {
                case 0: 
                    float ver = BinaryPrimitives.ReadSingleBigEndian(span.Slice(0, 4));
                    int width = BinaryPrimitives.ReadInt32BigEndian(span.Slice(4, 4));
                    int height = BinaryPrimitives.ReadInt32BigEndian(span.Slice(8, 4));
                    mapData.WorldSize[0] = width;
                    mapData.WorldSize[1] = height;
                    break;
                case 1:
                    mapData.Author = System.Text.Encoding.UTF8.GetString(span);
                    break;
                case 2:
                    mapData.Desc = System.Text.Encoding.UTF8.GetString(span);
                    break;
                case 3:
                    float gameVer = BinaryPrimitives.ReadSingleBigEndian(span);
                    mapData.MapForGameVersion = gameVer;
                    break;
                case 4:
                    int offset = 0;
                    byte count = span[offset++];

                    for (int i = 0; i < count; i++) {
                        byte nameLen = span[offset++];
                        string name = System.Text.Encoding.UTF8.GetString(span.Slice(offset, nameLen));
                        offset += nameLen;
                        byte id = span[offset++];
                        groundList[name] = id;
                        Console.WriteLine($"{name} -> {id}");
                    }
                    break;
            }
        } 

        return mapData;
    }
}

public class StructuredMapData {
    public string? Author {get; set;}
    public string? Desc {get; set;}
    public float GameVersion {get; set;}
    public int[] WorldSize {get; set;} = [0, 0];
    public float MapForGameVersion {get; set;}
    public byte[] Grounds { get; set; } = [];
    public string Buildings {get; set;} = "";
    Dictionary<int, string> idToGround = MapControl.groundList.ToDictionary(pair => pair.Value, pair => pair.Key);

    public (string?, string?, TileCell[,]) Convert() {
        if (TileRegistry.Instance == null) {
            throw new Exception("TileRegistry Not Found");
        }

        int width = WorldSize[0];
        int height = WorldSize[1];
        TileCell[,] OriginMap = new TileCell[width, height];

        for (int x = 0; x<WorldSize[0]; x++) {
            for (int y = 0; y<WorldSize[1]; y++) {
                int groundId = Grounds[y * width + x];

                if (idToGround.TryGetValue((int)groundId, out string? groundName)) {
                    if (TileRegistry.Instance.Grounds.TryGetValue(groundName, out var grassDef)) {
                        OriginMap[x, y] = new TileCell(grassDef, null);
                    }
                }
            }
        }
        return (Author, Desc, OriginMap);
    }
}