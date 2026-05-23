using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/CountEntitiesTrigger")]
[Tracked(false)]
public class CountEntitiesTrigger(EntityData e, Vector2 offset) : Trigger(e, offset)
{
    internal readonly HashSet<string> Names = [.. e.String("Names", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    internal readonly string CounterName = e.String("Counter", "");

    public void CountEntities(Level level) {
        int count = 0;
        foreach (Entity entity in level.Entities)
            if (Names.Overlaps(ScugHelperModule.GetNamesOfEntity(entity))) count++;
        level.Session.SetCounter(CounterName, count);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        CountEntities(player.level);
    }
    public override void OnStay(Player player) {
        base.OnStay(player);
        CountEntities(player.level);
    }
}
