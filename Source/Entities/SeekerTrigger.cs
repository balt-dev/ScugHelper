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
    bool State = data.Bool("State", true);
    public override void Awake(Scene scene) {
        if (!Global) return;
        if (scene.Tracker.GetEntity<Player>() is not Player player) return;
        Apply(player);
        
    }
    public override void OnEnter(Player player) {
        if (Global) return;
        Apply(player);
    }

    private void Apply(Player player) {
        var comp = player.Get<PlayerSeekerComponent>();
        if (State && comp is null)
            player.Add(new PlayerSeekerComponent(playSound: false));
        else if (!State && comp is PlayerSeekerComponent pleeker)
            player.Remove(pleeker);
    }
}
#nullable restore
