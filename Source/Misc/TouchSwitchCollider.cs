using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Monocle;

namespace Celeste.Mod.ScugHelper;

[Tracked(false)]
internal class TouchSwitchCollider(Action<TouchSwitch>? onCollide = null) : Component(active: false, visible: false) {
    public Action<TouchSwitch> OnCollide = onCollide ?? (static (sw) => sw.TurnOn());

    public void Check(TouchSwitch obj) {
        if (obj.CollideCheck(Entity))
            OnCollide?.Invoke(obj);
    }
    [OnLoad] internal static void LoadHooks() => On.Celeste.TouchSwitch.Update += OnTouchSwitchUpdate;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.TouchSwitch.Update -= OnTouchSwitchUpdate;

    private static void OnTouchSwitchUpdate(On.Celeste.TouchSwitch.orig_Update orig, TouchSwitch self) {
        foreach (TouchSwitchCollider coll in self.Scene.Tracker.GetComponents<TouchSwitchCollider>())
            coll.Check(self);
        orig(self);
    }
}