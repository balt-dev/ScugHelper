using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Linq;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

internal class UnforgivingCoreKillerColliderList(Entity self) : AbstractEntityColliderList(self) {
    protected override bool CheckEntity(Entity entity) {
        if (entity is not Player player) return true;
        if (SaveData.Instance.Assists.Invincible) return true;
        player.Die((player.Center - Entity.Center).SafeNormalize());
        return true;
    }
}

// Why are these separate entities.

[TrackedAs(typeof(FireBarrier))]
[CustomEntity("ScugHelper/UnforgivingFireBarrier")]
public class UnforgivingFireBarrier(EntityData data, Vector2 offset) : FireBarrier(data.Position + offset, data.Width, data.Height) {
    public override void Added(Scene scene) {
        base.Added(scene);
        Components.RemoveAll<PlayerCollider>();
        solid.Collider = new UnforgivingCoreKillerColliderList(solid);
    }
}

[TrackedAs(typeof(IceBlock))]
[CustomEntity("ScugHelper/UnforgivingIceBlock")]
public class UnforgivingIceBlock(EntityData data, Vector2 offset) : IceBlock(data.Position + offset, data.Width, data.Height) {
    public override void Added(Scene scene) {
        base.Added(scene);
        Components.RemoveAll<PlayerCollider>();
        solid.Collider = new UnforgivingCoreKillerColliderList(solid);
    }
}
