using System;
using System.Collections.Generic;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper;

class EntityCollider<E>(Action<E> onCollide) : Component(false, false) where E: Entity {
    private readonly Action<E> onCollide = onCollide;

    public override void Added(Entity entity) {
        base.Added(entity);
    }
    
    public override void Removed(Entity entity) {
        base.Removed(entity);
    }
    public override void EntityRemoved(Scene scene) {
        base.EntityRemoved(scene);
    }

    public void Check(Entity obj) {
        if (obj is E ent && Entity.CollideCheck(ent))
            onCollide.Invoke(ent);
    }
}

[Tracked] class SpringCollider(Action<Spring> onCollide) : EntityCollider<Spring>(onCollide) { }
[Tracked] class TouchSwitchCollider(Action<TouchSwitch> onCollide) : EntityCollider<TouchSwitch>(onCollide) { }

internal static class EntityColliderHooks {
    [OnLoad] internal static void LoadHooks() {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += OnSpringCtor;
        On.Celeste.TouchSwitch.ctor_Vector2 += OnTouchSwitchCtor;
    }

    private static void OnTouchSwitchCtor(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position) {
        orig(self, position);
        self.Add(new ColliderListenerComponent<TouchSwitch, TouchSwitchCollider>());
    }

    private static void OnSpringCtor(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse){
        orig(self, position, orientation, playerCanUse);
        self.Add(new ColliderListenerComponent<Spring, SpringCollider>());
    }

    private class ColliderListenerComponent<E, T>() : Component(true, false) where T: EntityCollider<E> where E: Entity {
        public override void Update() {
            base.Update();
            foreach (T coll in Scene.Tracker.GetComponents<T>()) coll.Check(Entity);
        }
    }
}