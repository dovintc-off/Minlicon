from pathlib import Path
content_dir = Path(__file__).resolve().parent.parent / "Content"
generate_folders = ["assets-raw", "assets"]
mgcb_file = content_dir / "Content.mgcb"
MARKER = "#-------------------------------- Generated ---------------------------------#"
EXTENSIONS = {".png", ".jpg", ".jpeg"}

if mgcb_file.exists():
    with open(mgcb_file, "r", encoding="utf-8") as file: lines = file.readlines()
    new_content = []

    for line in lines:
        new_content.append(line)

        if line.rstrip("\r\n") == MARKER:
            break
    else:
        raise RuntimeError("Generated marker not found in Content.mgcb")

    for folder in generate_folders:
        folder_path = content_dir / folder

        if not folder_path.exists():
            continue

        image_files = sorted(
            p for p in folder_path.rglob("*")
            if p.is_file() and p.suffix.lower() in EXTENSIONS
        )

        for img in image_files:
            relative_path = img.relative_to(Path(content_dir)).as_posix()

            new_content.extend([
                f"#begin {relative_path}\n",
                "/importer:TextureImporter\n",
                "/processor:TextureProcessor\n",
                f"/build:{relative_path}\n",
                "\n"
            ])

    with open(mgcb_file, "w", encoding="utf-8") as file:
        file.writelines(new_content)

    print("MGCB generated successfully.")
