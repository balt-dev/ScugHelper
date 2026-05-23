using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/GlobalEntityActionListener")]
public class GlobalEntityActionListener : Entity, IAction {
    public readonly string[] Groups;
    internal List<PlayerCollider> colliders = [];

    public GlobalEntityActionListener(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Collider = new Hitbox(data.Width, data.Height);
        Groups = IAction.GetGroups(data);
    }

    public void ActionUpdate(Level level) {
        foreach (PlayerCollider coll in colliders) {
            coll.Entity.Scene = level;
            coll.Update();
        }
    }
    public void Alert(Level level) {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        foreach (PlayerCollider coll in colliders) {
            coll.Entity.Scene = level;
            var oldPos = coll.Entity.Position;
            coll.Entity.Position = player.Position;
            coll.OnCollide(player);
            coll.Entity.Position = oldPos;
        }
    }
}
