using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/FreezeAction")]
public class FreezeAction(EntityData data, Vector2 _) : Entity(), IAction
{
    readonly float FreezeTime = data.Float("Duration");
    public void Alert(Level level) {
        Celeste.Freeze(FreezeTime);
    }
}