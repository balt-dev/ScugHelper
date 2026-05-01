using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;


[Tracked]
[CustomEntity("ScugHelper/SeekerTrigger")]
public class SeekerTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    bool Global = data.Bool("Global");
    public override void Awake(Scene scene) {
        if (!Global) return;
        if (scene.Tracker.GetEntity<Player>() is not Player player) return;
        if (player.Get<PlayerSeekerComponent>() is null)
            player.Add(new PlayerSeekerComponent(playSound: false));
        
    }
    public override void OnEnter(Player player)
    {
        if (Global) return;
        if (player.Get<PlayerSeekerComponent>() is null)
        {
            player.Add(new PlayerSeekerComponent());
        }
    }
}
#nullable restore
