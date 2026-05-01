using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/EntityGlobalizer")]
public class EntityGlobalizer : Entity, IAction {
    internal List<Entity> entities = [];

    public EntityGlobalizer(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
    }

    public void ActionUpdate(Level level)
    {
        foreach (Entity ent in entities)
        {
            ent.Scene = level;
            if (level.Tracker.GetEntity<Player>() is Player player) ent.Position = player.Position;
            ent.Update();
        }
    }
}
