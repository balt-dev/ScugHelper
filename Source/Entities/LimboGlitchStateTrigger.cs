using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/LimboGlitchStateTrigger")]
public class LimboGlitchStateTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    readonly bool LockStateMachine = data.Bool("LockStateMachine", true);

    public override void OnEnter(Player player)
    {
        var dummyEntity = new LimboDummyEntity(player.Position);
        player.Sprite.Entity = dummyEntity;
        player.Hair.Entity = dummyEntity;
        player.Light.Entity = dummyEntity;
        player.Dead = true;
        player.Depth = -1000000;
        if (LockStateMachine)
        {
            player.StateMachine.State = Player.StNormal;
            player.StateMachine.Locked = true;
        }
        player.Collidable = false;
    }
}

[Tracked]
internal class LimboDummyEntity(Vector2 pos) : Entity(pos) { }
