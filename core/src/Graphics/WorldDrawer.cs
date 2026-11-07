using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.TileSystem;

namespace Minlicon.Core.Graphics;

public class WorldDrawer 
{
    private readonly SpriteBatch _spriteBatch;
    private readonly TileSystem.TileSystem _tileSystem;
    private readonly Texture2D _atlasTexture;
    private readonly TileRegistry _registry;
    private readonly int _tileSize;

    public WorldDrawer(SpriteBatch spriteBatch, TileSystem.TileSystem tileSystem, AtlasGenerator.AtlasType atlas, TileRegistry registry, int tileSize = 32) {
        _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        _tileSystem = tileSystem ?? throw new ArgumentNullException(nameof(tileSystem));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _tileSize = tileSize;
        _atlasTexture = atlas?.AtlasTexture ?? throw new ArgumentException("Atlas texture cannot be null");
    }

    public void Draw(float cameraX, float cameraY, float zoom, Viewport viewport, Point hoveredTile) {
        float screenCenterX = viewport.Width / 2f;
        float screenCenterY = viewport.Height / 2f;

        float halfViewWidth = screenCenterX / zoom;
        float halfViewHeight = screenCenterY / zoom;

        int minTileX = Math.Max(0, (int)Math.Floor((cameraX - halfViewWidth) / _tileSize));
        int maxTileX = Math.Min(99, (int)Math.Floor((cameraX + halfViewWidth) / _tileSize));
        int minTileY = Math.Max(0, (int)Math.Floor((cameraY - halfViewHeight) / _tileSize));
        int maxTileY = Math.Min(99, (int)Math.Floor((cameraY + halfViewHeight) / _tileSize));

        for (int x = minTileX; x <= maxTileX; x++) {
            for (int y = minTileY; y <= maxTileY; y++) {
                TileCell cell = _tileSystem.Get(x, y);
                Vector2 tilePos = new Vector2(x * _tileSize, y * _tileSize);
                
                Color tileColor = (x == hoveredTile.X && y == hoveredTile.Y) ? new Color(255, 255, 200) : Color.White;
                GroundDefinition groundDef = cell.Ground;

                if (groundDef.Variants != null && groundDef.Variants.Length > 0) {
                    int variantIndex = Math.Clamp(cell.TextureVariant, 0, groundDef.Variants.Length - 1);
                    Rectangle srcRect = groundDef.Variants[variantIndex];

                    Vector2 origin = new Vector2(srcRect.Width / 2f, srcRect.Height / 2f);
                    Vector2 tileCenterPosition = tilePos + new Vector2(_tileSize / 2f, _tileSize / 2f);
                    float radians = MathHelper.ToRadians(cell.Rotate);

                    _spriteBatch.Draw(_atlasTexture, tileCenterPosition, srcRect, tileColor, radians, origin, 1f, SpriteEffects.None, 0f);
                }

                if (cell.Building != null && cell.Building.X == x && cell.Building.Y == y) {
                    DrawBuildingInstance(cell.Building, tileColor);
                }
            }
        }
    }

    private void DrawBuildingInstance(BuildingInstance building, Color color) {
        if (building.Definition.Drawer?.Layers == null || building.Definition.Drawer.Layers.Count == 0) return;

        Vector2 baseTilePos = new Vector2(building.X * _tileSize, building.Y * _tileSize);
        Vector2 origin = new Vector2(_tileSize / 2f, _tileSize / 2f);
        Vector2 centerPosition = baseTilePos + origin;
        float radians = MathHelper.ToRadians(building.Rotate * 90f);

        foreach (var layer in building.Definition.Drawer.Layers) {
            if (layer.AtlasRect == Rectangle.Empty) continue;
            _spriteBatch.Draw(_atlasTexture, centerPosition, layer.AtlasRect, color, radians, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}
