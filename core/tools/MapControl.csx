using System.Buffers.Binary;

Read("world.mnlc");

public void Read(string file) {
    Dictionary<string, int> groundList = new Dictionary<string, int>(){};

    if (!File.Exists(file)) throw new FileNotFoundException("Файл не найден", file);

    using var f = File.OpenRead(file);
    if (f.Length < 4) return;

    using var reader = new BinaryReader(f);
    string signature = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));

    if (signature != "MNLC") return;

    while (f.Position < f.Length) {
        byte BlockType = reader.ReadByte();
        if (f.Position >= f.Length) break;

        if (BlockType == 5) {
            if (f.Position + 4 > f.Length) break;
            uint mapDataLength = BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));
            if (f.Position + mapDataLength > f.Length) break;

            byte[] sequentialWorld = reader.ReadBytes((int)mapDataLength);
            
            Console.WriteLine($"Загружено тайлов подряд: {sequentialWorld.Length}");
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
                Console.WriteLine($"{ver} {width} {height}");
                break;
            case 1:
            case 2:
                System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(span));
                break;
            case 3:
                float gameVer = BinaryPrimitives.ReadSingleBigEndian(span);
                System.Console.WriteLine(gameVer);
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
}

public void Write() {
    
}