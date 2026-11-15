namespace Minlicon.Core.Scenes;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public class SceneManager {
    private readonly Dictionary<string, Scene> _scenes = new();

    public Scene? ActualScene {get; private set;}

    public void Reg(Scene _newScene) {
        _scenes[_newScene.Name] = _newScene;
    }

    public void Next(string name) {
        if (!_scenes.TryGetValue(name, out var next)) {
            throw new Exception($"Scene {name} is not found");
        }
        if (ActualScene == next) return;
        ActualScene?.Exit(); ActualScene?.Unload();
        ActualScene = next;
        ActualScene.Load(); ActualScene.Enter();
    }

    public void Draw(GameTime gt) {
        if (ActualScene == null) throw new Exception($"ActualScene is null");
        ActualScene.Draw(gt);
    }

    public void Update(GameTime gt) {
        if (ActualScene == null) throw new Exception($"ActualScene is null");
        ActualScene.Update(gt);
    }
}