using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;


[Tracked]
[CustomEntity("ScugHelper/UnstuckTrigger")]
public class UnstuckTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    public readonly int AmountX = data.Int("AmountX");
    public readonly int AmountY = data.Int("AmountY");
    public override void OnStay(Player player) {
        if (!player.CollideCheck<Solid>(player.Position)) return;
        var sol = new Solid(Vector2.Zero, 0, 0, false);
        if (!player.TrySquishWiggle(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = player.Position }, AmountX, AmountY))
            player.Die(Vector2.Zero);
    }
}
#nullable restore
