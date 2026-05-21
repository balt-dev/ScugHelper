using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/FauxLimboTrigger")]
public class FauxLimboTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public override void OnEnter(Player player)
    {
        player.Dead = true;
        player.Depth = -1000000;
        player.StateMachine.Locked = true;
        player.Collidable = false;
    }
}
