using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/ExitLimboGlitchStateTrigger")]
public class ExitLimboGlitchStateTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public override void OnEnter(Player player) {
        player.Dead = false;
        player.Depth = 0;
        player.Collidable = true;
        player.StateMachine.Locked = false;
        player.Scene.Tracker.GetEntities<LimboDummyEntity>().ForEach(ent => ent.RemoveSelf());
        player.Sprite.Entity = player;
        player.Hair.Entity = player;
        player.Light.Entity = player;
        if (Scene is not null) player.Scene = Scene;
    }
}
