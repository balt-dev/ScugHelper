using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/StaticHitboxTrigger")]
public class StaticHitboxTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    private StaticHitboxComponent? comp;

    public override void OnEnter(Player player) => player.Add(comp = new StaticHitboxComponent(player.Collider));
    public override void OnLeave(Player player) => player.Remove(comp);

    internal class StaticHitboxComponent(Collider collider): Component(false, false) {
        internal Collider Collider = collider;
    }

    [OnLoad] internal static void LoadHooks() {
        On.Celeste.Player.Update += OnPlayerUpdate;
        On.Celeste.Player.OnCollideH += OnPlayerOnCollideH;
        On.Celeste.Player.OnCollideV += OnPlayerOnCollideV;
    }
    [OnUnload] internal static void UnloadHooks() {
        On.Celeste.Player.Update -= OnPlayerUpdate;
        On.Celeste.Player.OnCollideH -= OnPlayerOnCollideH;
        On.Celeste.Player.OnCollideV -= OnPlayerOnCollideV;
    }

    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        orig(self);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) self.Collider = WithLimbo(comp.Collider);
    }

    private static void OnPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
        orig(self, data);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) self.Collider = WithLimbo(comp.Collider);
    }

    private static void OnPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
        orig(self, data);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) self.Collider = WithLimbo(comp.Collider);
    }

    private static Collider WithLimbo(Collider collider) {
        if (collider is LimboRefill.LimboColliderList lcl && LimboRefill.LimboTimer <= 0f) return lcl.OriginalCollider;
        if (collider is not LimboRefill.LimboColliderList && LimboRefill.LimboTimer > 0f) return new LimboRefill.LimboColliderList(collider);
        return collider;
    }
}
