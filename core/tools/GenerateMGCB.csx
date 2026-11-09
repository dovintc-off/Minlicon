string _contentDir = Path.Combine(Directory.GetCurrentDirectory(), "core", "Content");
string _MGCBFile = Path.Combine(_contentDir, "Content.mgcb");
string _indexFile = Path.Combine(_contentDir, "index.txt");
string _marker = "#-------------------------------- Generated ---------------------------------#";
string[] _folders = {"assets-raw", "assets"};
string[] _ext = {".png", ".jpg", ".jpeg"};
File.WriteAllText(_indexFile, "");

if (Path.Exists(_MGCBFile)) {
    List<string> newContent = new List<string>();
    string content = File.ReadAllText(_MGCBFile);

    foreach (var line in content.Split(new string[] {"\r\n","\n"}, StringSplitOptions.None)){
        newContent.Add(line);
        if (line == _marker) break;
    }

    foreach (var fold in _folders) {

        string FolderPath = Path.Combine(_contentDir, fold);
        if (!Directory.Exists(FolderPath)) {
            continue;
        }

        var ImagesFile = Directory.EnumerateFiles(FolderPath, "*", SearchOption.AllDirectories)
            .Where(file =>  _ext.Contains(Path.GetExtension(file).ToLower()))
            .OrderBy(file => file).ToList();

        foreach (var img in ImagesFile) {
            string relativePath = Path.GetRelativePath(_contentDir, img).Replace('\\', '/');

            newContent.AddRange(new[] {
                $"\n#begin {relativePath}",
                "/importer:TextureImporter",
                "/processor:TextureProcessor",
                $"/build:{relativePath}"
            });

            File.AppendAllText(_indexFile, Path.ChangeExtension(relativePath, null)+"\n");
        }
    }

    File.WriteAllLines(_MGCBFile, newContent);
    System.Console.WriteLine("MGCB generated successfully.");
}