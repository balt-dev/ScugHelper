using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/ForwardAction")]
public class ForwardAction(EntityData data, Vector2 _) : Entity(), IAction
{
    public readonly string[] Targets = IAction.GetTargets(data);
    public void Alert(Level level) {
        ActionManager.AlertActions(Targets, level);
    }
}
