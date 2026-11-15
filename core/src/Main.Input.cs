using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

namespace Minlicon.Core;

public partial class Main {

    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    
    internal void ToggleDebug() {
        _showDebug = !_showDebug;
    }

    protected virtual void HandlePlatformInput(GameTime gameTime, KeyboardState keyboard, MouseState mouse) {}

    protected virtual void ConfigurePlatformGraphics() {}

    protected void InitializeInput() {
        TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Pinch;
        _previousKeyboardState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    protected virtual Dictionary<string, string> LoadRegistryData() {
        return new Dictionary<string, string>();
    }
    
    protected virtual void DrawPlatformCursor(SpriteBatch spriteBatch, PlayOnMap scene) {}
}

public readonly struct InputState {
    public KeyboardState Keyboard { get; }
    public MouseState Mouse { get; }
    public KeyboardState PreviousKeyboard { get; }
    public MouseState PreviousMouse { get; }

    public InputState(KeyboardState keyboard, MouseState mouse, KeyboardState previousKeyboard, MouseState previousMouse) {
        Keyboard = keyboard;
        Mouse = mouse;
        PreviousKeyboard = previousKeyboard;
        PreviousMouse = previousMouse;
    }
}