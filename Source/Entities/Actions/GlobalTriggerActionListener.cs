using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/GlobalTriggerActionListener")]
public class GlobalTriggerActionListener(EntityData data, Vector2 _) : Entity(), IAction {
    public readonly string[] Groups = IAction.GetGroups(data);
    internal List<Trigger> triggers = [];
    public void ActionUpdate(Level level) {
        foreach (Trigger trigger in triggers)
            trigger.Update();
    }
    public void Alert(Level level)
    {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        foreach (Trigger trigger in triggers) { trigger.OnEnter(player); trigger.OnStay(player); trigger.OnLeave(player); }
    }
}
