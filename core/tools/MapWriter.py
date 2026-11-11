file_path = "world.mnlc"

import struct

with open(file_path, "wb") as f:
    # Step 1 
    
    header = struct.pack(">fii", 0.1, 100, 100)
    f.write(b"MNLC\x00" + struct.pack("B", len(header)) + header)
    
    # Step 2
    
    author = "Dovintc"
    author_B = author.encode('utf-8')
    desc = "First map on Minlicon ever"
    desc_B = desc.encode('utf-8')
    game_ver = 0.2
    game_ver_B = struct.pack(">f", game_ver)
    
    f.write(b"\x01" + struct.pack("B", len(author_B)) + author_B)
    f.write(b"\x02" + struct.pack("B", len(desc_B)) + desc_B)
    f.write(b"\x03" + struct.pack("B", len(game_ver_B)) + game_ver_B)
    
    # Step 3 
    
    GROUNDSLIST = {"Grass": 1, "Sand": 2}
    
    # data = [длинна куска дальше][то что заменяем][цифра чем заменяем]
    grounds_data = bytearray()
    grounds_data += struct.pack("B", len(GROUNDSLIST))
    
    for name, ground_id in GROUNDSLIST.items():
        name_bytes = name.encode('utf-8')
        grounds_data += struct.pack("B", len(name_bytes))
        grounds_data += name_bytes
        grounds_data += struct.pack("B", ground_id)
    
    f.write(b"\x04" + struct.pack("B", len(grounds_data)) + grounds_data)
    
    # Step 4
    
    WORLD = [[1 if row % 2 == 0 else 2 for _ in range(100)] for row in range(100)]

    flat_world = [tile for row in WORLD for tile in row]
    world_bytes = bytes(flat_world)

    f.write(b"\x05" + struct.pack(">I", len(world_bytes)) + world_bytes)


try:
    with open(file_path, "rb") as f:    
        bytes_list = list(f.read())
        
    signature = bytes(bytes_list[:4]).decode('ascii')

    if signature == "MNLC":
        if bytes_list[4] == 0: 
            data_length = bytes_list[5] 
            end_index = 6 + data_length 
            raw_data = bytes(bytes_list[6:end_index])
            version, width, height = struct.unpack(">fii", raw_data)
            
            print(f"Версия: {version}, Ширина: {width}, Высота: {height}")
        print(bytes_list[end_index:])
        
            
except Exception as e:
    print(f"Произошла ошибка: {e}")
