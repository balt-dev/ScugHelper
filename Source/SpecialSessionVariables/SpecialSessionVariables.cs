using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Monocle;
using MonoMod.RuntimeDetour;
#nullable enable

namespace Celeste.Mod.ScugHelper.SpecialSessionVariables;

public static class SpecialSessionVariables
{
    static readonly Dictionary<string, SpecialFlag> flags = new([
        new("ScugHelper.PlayerDead", new PlayerDeadFlag()),
        new("ScugHelper.HasGolden", new HasGoldenFlag()),
        new("ScugHelper.RestartedFromGolden", new RestartedFromGoldenFlag()),
        new("ScugHelper.StartedFromBeginning", new StartedFromBeginningFlag()),
        new("ScugHelper.PlayerOnGround", new PlayerOnGroundFlag()),
        new("ScugHelper.PlayerOnSafeGround", new PlayerOnSafeGroundFlag()),
        new("ScugHelper.PlayerDashAttacking", new PlayerDashAttackingFlag()),
        new("ScugHelper.IsPlayerSeeker", new IsPlayerSeekerFlag()),
        new("ScugHelper.DreamBlocksEnabled", new DreamBlocksEnabledFlag()),
        new("ScugHelper.HasMidair", new HasMidairFlag()),
        new("ScugHelper.HasOvercharge", new HasOverchargeFlag()),
        new("ScugHelper.InLimbo", new HasLimboFlag()),
        new("ScugHelper.SaveQuitDisabled", new SaveQuitDisabledFlag()),
        new("ScugHelper.PlayerHolding", new PlayerHoldingFlag()),
        new("ScugHelper.PlayerDucking", new PlayerDuckingFlag()),
        new("ScugHelper.GravityHelper.PlayerInverted", new PlayerInvertedFlag()),
        new("ScugHelper.FrostHelper.Enabled", new FrostHelperEnabledFlag()),
        new("ScugHelper.GravityHelper.Enabled", new GravityHelperEnabledFlag()),
        new("ScugHelper.InBooster", new InBoosterFlag()),
    ]);
    static readonly Dictionary<string, SpecialCounter> counters = new([
        new("ScugHelper.DeathCount", new DeathCounter()),
        new("ScugHelper.DeathHereCount", new HereDeathCounter()),
        new("ScugHelper.PlayerState", new PlayerStateCounter()),
        new("ScugHelper.PlayerDashes", new PlayerDashesCounter()),
        new("ScugHelper.PlayerMaxDashes", new PlayerMaxDashesCounter()),
        new("ScugHelper.PlayerTotalDashes", new PlayerTotalDashesCounter()),
        new("ScugHelper.EpochTime", new EpochTimeCounter()),
        new("ScugHelper.LevelX", new LevelXCounter()),
        new("ScugHelper.LevelY", new LevelYCounter()),
        new("ScugHelper.LevelWidth", new LevelWidthCounter()),
        new("ScugHelper.LevelHeight", new LevelHeightCounter()),
        new("ScugHelper.FPS", new FPSCounter()),
        new("ScugHelper.CassetteBlockIndex", new CassetteBlockIndexCounter()),
        new("ScugHelper.CoreMode", new CoreModeCounter()),
    ]);
    static readonly Dictionary<string, SpecialSlider> sliders = new([
        new("ScugHelper.SessionTime", new SessionTimeSlider()),
        new("ScugHelper.TimeRate", new TimeRateSlider()),
        new("ScugHelper.PlayerX", new PlayerXSlider()),
        new("ScugHelper.PlayerY", new PlayerYSlider()),
        new("ScugHelper.PlayerSpeedX", new PlayerSpeedXSlider()),
        new("ScugHelper.PlayerSpeedY", new PlayerSpeedYSlider()),
        new("ScugHelper.PlayerSubpixelX", new PlayerSubpixelXSlider()),
        new("ScugHelper.PlayerSubpixelY", new PlayerSubpixelYSlider()),
        new("ScugHelper.PlayerStamina", new PlayerStaminaSlider()),
    ]);

    private static Hook OnSliderObjectGetValue = null!;
    private static Hook OnSliderObjectSetValue = null!;

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Session.GetFlag += OnGetFlag;
        On.Celeste.Session.GetCounter += OnGetCounter;
        On.Celeste.Session.GetSlider += OnGetSlider;
        On.Celeste.Session.SetFlag += OnSetFlag;
        On.Celeste.Session.SetCounter += OnSetCounter;
        On.Celeste.Session.IncrementCounter += OnIncrementCounter;
        OnSliderObjectGetValue = new(typeof(Session.Slider).GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance)!, OnSliderObjectGet);
        OnSliderObjectSetValue = new(typeof(Session.Slider).GetMethod("set_Value", BindingFlags.Public | BindingFlags.Instance)!, OnSliderObjectSet);
        On.Celeste.Level.Update += OnUpdate;
    }

    static bool NoOverride;
    private static void OnUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        // Annoyingly slow but we do this to support legacy stuff
        NoOverride = true;
        foreach (var kvp in flags) self.Session.SetFlag(kvp.Key, kvp.Value.GetValue(self));
        foreach (var kvp in counters) self.Session.SetCounter(kvp.Key, kvp.Value.GetValue(self));
        foreach (var kvp in sliders) self.Session.SetSlider(kvp.Key, kvp.Value.GetValue(self));
        NoOverride = false;
        orig(self);
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Session.GetFlag -= OnGetFlag;
        On.Celeste.Session.GetCounter -= OnGetCounter;
        On.Celeste.Session.GetSlider -= OnGetSlider;
        On.Celeste.Session.SetFlag -= OnSetFlag;
        On.Celeste.Session.IncrementCounter -= OnIncrementCounter;
        OnSliderObjectGetValue.Dispose();
        OnSliderObjectSetValue.Dispose();
    }

    private static bool OnGetFlag(On.Celeste.Session.orig_GetFlag orig, Session self, string flag) {
        if (!NoOverride && flag is not null && Engine.Scene is Level level && flags.TryGetValue(flag, out var specialFlag)) return specialFlag.GetValue(level);
        else return orig(self, flag);
    }

    private static int OnGetCounter(On.Celeste.Session.orig_GetCounter orig, Session self, string counter) {
        if (!NoOverride && counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) return specialCounter.GetValue(level);
        else return orig(self, counter);
    }

    private static float OnGetSlider(On.Celeste.Session.orig_GetSlider orig, Session self, string slider) {
        if (!NoOverride && slider is not null && Engine.Scene is Level level && sliders.TryGetValue(slider, out var specialSlider)) return specialSlider.GetValue(level);
        else return orig(self, slider);
    }

    private static void OnSetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool value) {
        if (!NoOverride && flag is not null && Engine.Scene is Level level && flags.TryGetValue(flag, out var specialFlag)) specialFlag.SetValue(level, value);
        else orig(self, flag, value);
    }

    private static void OnSetCounter(On.Celeste.Session.orig_SetCounter orig, Session self, string counter, int value) {
        if (!NoOverride && counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) specialCounter.SetValue(level, value);
        else orig(self, counter, value);
    }

    private static void OnIncrementCounter(On.Celeste.Session.orig_IncrementCounter orig, Session self, string counter) {
        if (!NoOverride && counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) specialCounter.SetValue(level, specialCounter.GetValue(level) + 1);
        else orig(self, counter);
    }

    private static float OnSliderObjectGet(Func<Session.Slider, float> orig, Session.Slider self) {
        if (!NoOverride && sliders.TryGetValue(self.Name, out var specialSlider))
            return (Engine.Scene is Level level) ? specialSlider.GetValue(level) : 0f;
        return orig(self);
    }

    private static void OnSliderObjectSet(Action<Session.Slider, float> orig, Session.Slider self, float value) {
        if (!NoOverride && sliders.TryGetValue(self.Name, out var specialSlider)) {
            if (Engine.Scene is Level level) specialSlider.SetValue(level, value);
        } else orig(self, value);
    }

    [Command("getflag", "Gets the value of a flag.")]
    internal static void CmdGetFlag(string name) {
        if (name is null || name == "") return;
        Engine.Commands.Log($"{name}: {(Engine.Scene as Level)?.Session.GetFlag(name)}");
    }
    [Command("getcounter", "Gets the value of a counter.")]
    internal static void CmdGetCounter(string name) {
        if (name is null || name == "") return;
        Engine.Commands.Log($"{name}: {(Engine.Scene as Level)?.Session.GetCounter(name)}");
    }
    [Command("getslider", "Gets the value of a slider.")]
    internal static void CmdGetSlider(string name) {
        if (name is null || name == "") return;
        Engine.Commands.Log($"{name}: {(Engine.Scene as Level)?.Session.GetSlider(name)}");
    }

    [Command("setflag", "Sets the value of a flag.")]
    internal static void CmdSetFlag(string name, bool value) { if (name is null || name == "") return; (Engine.Scene as Level)?.Session.SetFlag(name, value); }
    [Command("setcounter", "Sets the value of a counter.")]
    internal static void CmdSetCounter(string name, int value) { if (name is null || name == "") return; (Engine.Scene as Level)?.Session.SetCounter(name, value); }
    [Command("setslider", "Sets the value of a slider.")]
    internal static void CmdSetSlider(string name, float value) { if (name is null || name == "") return; (Engine.Scene as Level)?.Session.SetSlider(name, value); }
}
