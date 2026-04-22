using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SnowballAction")]
public class SnowballAction() : Entity(), IAction
{
    public void Alert(Level level)
    {
        Snowball snowball = [new OneshotSnowballComponent(true, false)];
        level.Add(snowball);
    }
}

internal class OneshotSnowballComponent(bool active, bool visible) : Component(active, visible)
{
    public override void Update() {
        base.Update();
        if ((Entity as Snowball).resetTimer > 0.5f) Entity.RemoveSelf();
    }
}
