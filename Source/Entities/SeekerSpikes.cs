using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Diagnostics;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/SeekerSpikes-Up", "ScugHelper/SeekerSpikes-Left", "ScugHelper/SeekerSpikes-Down", "ScugHelper/SeekerSpikes-Right")]
[Tracked]
public class SeekerSpikes : Spikes
{
    public SeekerSpikes(EntityData data, Vector2 offset) : base(data, offset, data.Name switch {
        "ScugHelper/SeekerSpikes-Up" => Directions.Up,
        "ScugHelper/SeekerSpikes-Left" => Directions.Left,
        "ScugHelper/SeekerSpikes-Down" => Directions.Down,
        "ScugHelper/SeekerSpikes-Right" => Directions.Right,
        _ => throw new UnreachableException()
    }) {
        Remove(pc);
        Add(new PlayerCollider(OnPlayer));
        Add(new SeekerCollider(ScugHelperModule.KillSeeker));
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Track(this);
    }
    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Untrack(this);
    }
    
    private void OnPlayer(Player player) {
        if (player.Get<PlayerSeekerComponent>() is PlayerSeekerComponent comp) {
            comp.disableDeath = false;
            player.Die(-player.Speed.SafeNormalize(Vector2.UnitY));
        }
    }
}
