using (var r = new BinaryReader(File.Open("world.mlc", FileMode.Open))) {
    string signature = new string(r.ReadChars(4));
    Console.WriteLine(signature);
}


// file_path = "world.mlc"

// import struct

// with open(file_path, "wb") as f:
//     header = struct.pack(">fii", 0.1, 100, 100)
//     f.write(b"MNLC\x00" + struct.pack("B", len(header)) + header)

// try:
//     with open(file_path, "rb") as f:    
//         bytes_list = list(f.read())
        
//     signature = bytes(bytes_list[:4]).decode('ascii')
    
//     if signature == "MNLC":
//         if bytes_list[4] == 0: 
//             data_length = bytes_list[5] 
            
//             end_index = 6 + data_length 
//             raw_data = bytes(bytes_list[6:end_index])
//             version, width, height = struct.unpack(">fii", raw_data)
            
//             print(f"Версия: {version}, Ширина: {width}, Высота: {height}")
            
// except Exception as e:
//     print(f"Произошла ошибка: {e}")
