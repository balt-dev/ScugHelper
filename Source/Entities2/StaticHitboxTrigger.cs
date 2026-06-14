using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/StaticHitboxTrigger")]
public class StaticHitboxTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    private StaticHitboxComponent? comp;

    public override void OnEnter(Player player) => player.Add(comp = new StaticHitboxComponent(player.Collider, player.hurtbox));
    public override void OnLeave(Player player) => player.Remove(comp);

    internal class StaticHitboxComponent(Collider collider, Hitbox hurtbox): Component(false, true) {
        internal Collider Collider = collider;
        internal Hitbox Hurtbox = hurtbox;
        public override void Render() {
            base.Render();
            Entity.DebugRender((Entity as Player)!.level.Camera);
        }
    }

    [OnLoad] internal static void LoadHooks() {
        On.Celeste.Player.Update += OnPlayerUpdate;
        On.Celeste.Player.OnCollideH += OnPlayerOnCollideH;
        On.Celeste.Player.OnCollideV += OnPlayerOnCollideV;
        Everest.Events.Level.OnLoadLevel += OnEverestLevelOnLoadLevel;
    }

    [OnUnload] internal static void UnloadHooks() {
        On.Celeste.Player.Update -= OnPlayerUpdate;
        On.Celeste.Player.OnCollideH -= OnPlayerOnCollideH;
        On.Celeste.Player.OnCollideV -= OnPlayerOnCollideV;
        Everest.Events.Level.OnLoadLevel -= OnEverestLevelOnLoadLevel;
    }

    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        orig(self);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) {
            self.Collider = WithLimbo(comp.Collider);
            self.hurtbox = comp.Hurtbox;
            self.Sprite.Scale = Vector2.One;
        }
    }

    private static void OnPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
        orig(self, data);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) {
            self.Collider = WithLimbo(comp.Collider);
            self.hurtbox = comp.Hurtbox;
            self.Sprite.Scale = Vector2.One;
        }
    }

    private static void OnPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
        orig(self, data);
        if (self.Get<StaticHitboxComponent>() is StaticHitboxComponent comp) {
            self.Collider = WithLimbo(comp.Collider);
            self.hurtbox = comp.Hurtbox;
            self.Sprite.Scale = Vector2.One;
        }
    }

    private static void OnEverestLevelOnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) =>
        level.Tracker.GetEntity<Player>()?.Components.RemoveAll<StaticHitboxComponent>();

    private static Collider WithLimbo(Collider collider) {
        if (collider is LimboRefill.LimboColliderList lcl && LimboRefill.LimboTimer <= 0f) return lcl.OriginalCollider;
        if (collider is not LimboRefill.LimboColliderList && LimboRefill.LimboTimer > 0f) return new LimboRefill.LimboColliderList(collider);
        return collider;
    }
}
