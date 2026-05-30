using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Utils;
using Celeste.Mod.ScugHelper.SpecialSessionVariables;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/EagerSSVController")]
[Tracked(false)]
public class EagerSSVController(EntityData data, Vector2 _) : Entity() {
    public readonly float UpdatePeriod = data.Float("UpdatePeriod", 0.05f);
    public override void Update() {
        base.Update();
        Level level = Scene as Level;
        
        if (!level.OnInterval(UpdatePeriod)) return;
        
        foreach (var kvp in SSV.flags) { if (kvp.Value.GetValue(level)) level.Session.Flags.Add(kvp.Key); else level.Session.Flags.Remove(kvp.Key); }
        foreach (var counter in level.Session.Counters)
            if (SSV.counters.TryGetValue(counter.Key, out var special)) counter.Value = special.GetValue(level);
        foreach (var slider in level.Session.Sliders)
            if (SSV.sliders.TryGetValue(slider.Key, out var special)) DynamicData.For(slider.Value).Set("_Value", special.GetValue(level)); // Does not trigger Everest event, because it explodes performance into a million pieces
    }
}
