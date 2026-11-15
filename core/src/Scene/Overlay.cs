namespace Minlicon.Core.Scenes;

using Microsoft.Xna.Framework;

public class Overlay {
    public UIManger UI {get;}

    public Overlay() {
        UI = new UIManger();
    }

    public void Update(GameTime gt) {
        UI.Update(gt);
    }

    public void Draw(GameTime gt) {
        UI.Draw(gt);
    }
}

public class UIManger {
    private readonly List<UIObject> UIObjects = new();

    public void New(UIObject obj) {
        UIObjects.Add(obj);
    }

    public void Update(GameTime gt){
        foreach (UIObject obj in UIObjects) {
            obj.Update(gt);
        }
    }

    public void Draw(GameTime gt){
        foreach (UIObject obj in UIObjects) {
            obj.Draw(gt);
        }
    }
}

public abstract class UIObject {
    public virtual void Update(GameTime gt) {}
    public virtual void Draw(GameTime gt) {}
}

public class InputUI : UIObject {
    
}