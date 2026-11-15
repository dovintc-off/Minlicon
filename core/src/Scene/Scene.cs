namespace Minlicon.Core.Scenes;

using Microsoft.Xna.Framework;

public abstract class Scene {
    public Overlay? Overlay {get; protected set;}
    protected Main main {get;}
    public string Name {get; private set;}

    protected Scene(Main game, string name) {
        main = game;
        Name = name;
    }

    public virtual void Unload() {}
    public virtual void Load() {}
    public virtual void Exit() {}
    public virtual void Enter() {}
    public virtual void Update(GameTime gt) {
        Overlay?.Update(gt);
    }
    public virtual void Draw(GameTime gt) {
        Overlay?.Draw(gt);
    }
}