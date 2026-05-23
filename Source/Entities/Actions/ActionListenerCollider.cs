using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/ActionListenerCollider")]
public class ActionListenerCollider : Entity
{
    readonly string[] Groups;
    readonly Vector2 CollidePos;

    public ActionListenerCollider(EntityData data, Vector2 offset): base(data.Position + offset) {
        Groups = IAction.GetGroups(data);
        CollidePos = data.FirstNodeNullable(offset) ?? throw new Exception("Action listener collider needs position to collide.");
        Add(new ActionListener(Groups, Alert));
    }

    internal void Alert(Level level) {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        foreach (PlayerCollider collider in Scene.CollideAllByComponent<PlayerCollider>(CollidePos))
            collider.OnCollide(player);
        foreach (Trigger trigger in Scene.CollideAll<Trigger>(CollidePos)) {
            trigger.OnEnter(player);
            trigger.OnStay(player);
            trigger.OnLeave(player);
        }
    }
}
