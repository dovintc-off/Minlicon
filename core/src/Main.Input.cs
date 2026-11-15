using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Minlicon.Core;

public partial class Main
{
    protected virtual void ConfigurePlatformGraphics() {}

    protected void InitializeInput()
    {
        TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Pinch;
    }

    protected virtual void HandlePlatformInput(
        PlayOnMap scene,
        GameTime gameTime,
        KeyboardState kState,
        MouseState mState,
        MouseState previousMouseState
    ) {}

    protected virtual Dictionary<string, string> LoadRegistryData()
    {
        return new Dictionary<string, string>();
    }

    protected virtual void DrawPlatformCursor(
        SpriteBatch spriteBatch,
        PlayOnMap scene
    ) {}
}
