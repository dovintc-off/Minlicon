using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minlicon.Core.Graphics;
using Minlicon.Core.Scenes;

namespace Minlicon.Core;

public partial class Main : Game {
    internal SpriteBatch SpriteBatch => _spriteBatch ?? throw new InvalidOperationException("SpriteBatch is not initialized");
    internal Dictionary<string, string> GetRegistryData() => LoadRegistryData();

    protected readonly SceneManager _sceneManager = new();
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch? _spriteBatch;

    public Main() {
        _graphics = new GraphicsDeviceManager(this);
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize() {
        ConfigurePlatformGraphics();
        _graphics.ApplyChanges();

        InitializeInput();
        _fpsWindowStartTimestamp = Stopwatch.GetTimestamp();

        _pixelBackground = new Texture2D(GraphicsDevice, 1, 1);
        _pixelBackground.SetData(new[] { Color.White });

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        FontRegistry.Initialize();
        _sceneManager.Reg(new PlayOnMap(this));
        _sceneManager.Next("PlayOnMap");
    }

    protected override void Update(GameTime gameTime) {
        _sceneManager.Update(gameTime);
        base.Update(gameTime);
    }
}