using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/CollideGate")]
public class CollideGate(EntityData data, Vector2 offset) : AbstractGate(data, offset) {
    private Vector2 CollidePosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Collide gate does not have a collide position node.");

    public override void OnTrigger(Player player)
    {
        Level level = SceneAs<Level>();
        foreach (PlayerCollider collider in CollideAllByComponent<PlayerCollider>(CollidePosition))
            collider.OnCollide(player);
    }
}
