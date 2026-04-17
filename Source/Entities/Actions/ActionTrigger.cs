using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/ActionTrigger")]
public class ActionTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public readonly string[] Targets = IAction.GetTargets(data);
    public readonly bool Once = data.Bool("Once");
    public override void OnEnter(Player player)
    {
        ActionManager.AlertActions(Targets, player.level);
        if (Once) RemoveSelf();
    }
}
