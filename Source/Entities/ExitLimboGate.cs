using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/ExitLimboGate")]
public class ExitLimboGate(EntityData data, Vector2 offset) : AbstractGate(data, offset) {
    public override void OnTrigger(Player player)
    {
        player.Dead = false;
        player.Depth = 0;
        player.StateMachine.Locked = false;
        player.Collidable = true;
    }
}
