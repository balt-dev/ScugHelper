using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/GlobalTriggerFlagListener")]
public class GlobalTriggerFlagListener(EntityData data, Vector2 _) : Entity(), IAction {
    public readonly string Flag = data.String("Flag");
    public readonly bool Invert = data.Bool("Invert");
    internal List<Trigger> triggers = [];
    private bool lastState = data.Bool("Invert");
    public void ActionUpdate(Level level) {
        foreach (Trigger trigger in triggers) {
            trigger.Scene = level;
            trigger.Update();
        }
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        bool flagState = level.Session.GetFlag(Flag) ^ Invert;
        if (flagState) {
            if (!lastState)
                foreach (Trigger trigger in triggers) trigger.OnEnter(player);
            foreach (Trigger trigger in triggers) trigger.OnStay(player);
        } else if (lastState) {
            foreach (Trigger trigger in triggers) trigger.OnLeave(player);
        }
        lastState = flagState;
    }
}
