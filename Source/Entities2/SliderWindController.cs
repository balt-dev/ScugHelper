using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SliderWindController")]
public class SliderWindController(EntityData data, Vector2 _, EntityID id) : Entity() {
    internal readonly string SliderX = data.String("SliderX", "");
    internal readonly string SliderY = data.String("SliderY", "");
    internal readonly float BaseX = data.Float("BaseX", 0f);
    internal readonly float BaseY = data.Float("BaseY", 0f);
    internal readonly bool Instant = data.Bool("Instant", false);

    internal Vector2 Strength => Scene is Level { Session: Session session }
        ? new(session.GetSlider(SliderX) + BaseX, session.GetSlider(SliderY) + BaseY)
        : Vector2.Zero;

    [OnLoad] internal static void LoadHooks() => On.Celeste.WindController.Update += OnWindControllerUpdate;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.WindController.Update -= OnWindControllerUpdate;

    private static void OnWindControllerUpdate(On.Celeste.WindController.orig_Update orig, WindController self) {
        if (self.Scene.Tracker.GetEntity<SliderWindController>() is {} windCtrl) {
            self.pattern = (WindController.Patterns) (-1);
            self.coroutine?.RemoveSelf();
            self.coroutine = null;
            self.targetSpeed = windCtrl.Strength;
            if (windCtrl.Instant)
                self.level.Wind = windCtrl.Strength;
            float length = self.level.Wind.Length();
            float strengthA = Math.Clamp(length / 600f, 0f, 1f);
            float strengthB = Math.Clamp((length - 600f) / 800f, 0f, 1f);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", -strengthA);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", strengthB);
        }
        orig(self);
    }
}
