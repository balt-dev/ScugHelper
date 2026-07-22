using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
#nullable enable

namespace Celeste.Mod.ScugHelper.SpecialSessionVariables;

public static class SSV
{
    internal static readonly Dictionary<string, SpecialFlag> flags;
    internal static readonly Dictionary<string, SpecialCounter> counters;
    internal static readonly Dictionary<string, SpecialSlider> sliders;

    static SSV() {
        flags = new([
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
            new("ScugHelper.RetryDisabled", new RetryDisabledFlag()),
            new("ScugHelper.PauseDisabled", new PauseDisabledFlag()),
            new("ScugHelper.PlayerHolding", new PlayerHoldingFlag()),
            new("ScugHelper.PlayerDucking", new PlayerDuckingFlag()),
            new("ScugHelper.GravityHelper.PlayerInverted", new PlayerInvertedFlag()),
            new("ScugHelper.FrostHelper.Enabled", new FrostHelperEnabledFlag()),
            new("ScugHelper.GravityHelper.Enabled", new GravityHelperEnabledFlag()),
            new("ScugHelper.InBooster", new InBoosterFlag()),
            new("ScugHelper.DashHeld", new ButtonHeldFlag(Input.Dash)),
            new("ScugHelper.JumpHeld", new ButtonHeldFlag(Input.Jump)),
            new("ScugHelper.GrabHeld", new ButtonHeldFlag(Input.Grab)),
            new("ScugHelper.CrouchDashHeld", new ButtonHeldFlag(Input.CrouchDash)),
            new("ScugHelper.TalkHeld", new ButtonHeldFlag(Input.Talk)),
        ]);
        counters = new([
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
        sliders = new([
            new("ScugHelper.SessionTime", new SessionTimeSlider()),
            new("ScugHelper.TimeRate", new TimeRateSlider()),
            new("ScugHelper.PlayerX", new PlayerXSlider()),
            new("ScugHelper.PlayerY", new PlayerYSlider()),
            new("ScugHelper.AimX", new AimXSlider()),
            new("ScugHelper.AimY", new AimYSlider()),
            new("ScugHelper.PlayerSpeedX", new PlayerSpeedXSlider()),
            new("ScugHelper.PlayerSpeedY", new PlayerSpeedYSlider()),
            new("ScugHelper.PlayerSubpixelX", new PlayerSubpixelXSlider()),
            new("ScugHelper.PlayerSubpixelY", new PlayerSubpixelYSlider()),
            new("ScugHelper.PlayerStamina", new PlayerStaminaSlider()),
            new("ScugHelper.LimboTimer", new LimboTimerSlider()),
            new("ScugHelper.CameraZoom", new CameraZoomSlider()),
            new("ScugHelper.CameraAngle", new CameraAngleSlider()),
        ]);
        ExtVarInterop.LoadVariables();
        CommunalHelperSSVInterop.LoadVariables();
    }

    private static Hook OnSliderObjectGetValue = null!;
    private static Hook OnSliderObjectSetValue = null!;

    [OnLoad]
    internal static void LoadHooks() {
        Utils.UninlineMethod((Session s) => s.GetFlag(null));
        Utils.UninlineMethod((Session s) => s.GetCounter(null));
        Utils.UninlineMethod((Session s) => s.GetSlider(null));
        Utils.UninlineMethod(typeof(Session.Slider).GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance)!);
        Utils.UninlineMethod(typeof(Session.Slider).GetMethod("set_Value", BindingFlags.Public | BindingFlags.Instance)!);
        On.Celeste.Session.GetFlag += OnGetFlag;
        On.Celeste.Session.GetCounter += OnGetCounter;
        On.Celeste.Session.GetSlider += OnGetSlider;
        On.Celeste.Session.SetFlag += OnSetFlag;
        On.Celeste.Session.SetCounter += OnSetCounter;
        On.Celeste.Session.IncrementCounter += OnIncrementCounter;
        OnSliderObjectGetValue = new(typeof(Session.Slider).GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance)!, OnSliderObjectGet);
        OnSliderObjectSetValue = new(typeof(Session.Slider).GetMethod("set_Value", BindingFlags.Public | BindingFlags.Instance)!, OnSliderObjectSet);
        //Everest.Events.LevelLoader.OnLoadingThread += OnLevelInit;
    }

    //private static void OnLevelInit(Level level) => FlushSSVStates(level);

    public static void FlushSSVStates(Level level) {
        foreach (var kvp in flags)
            if (kvp.Value.GetValue(level)) level.Session.Flags.Add(kvp.Key); else level.Session.Flags.Remove(kvp.Key);
        foreach (var counter in level.Session.Counters)
            if (counters.TryGetValue(counter.Key, out var special)) counter.Value = special.GetValue(level);
        foreach (var slider in level.Session.Sliders)
            // Does not trigger Everest event, because it explodes performance into a million pieces
            if (sliders.TryGetValue(slider.Key, out var special)) DynamicData.For(slider.Value).Set("_Value", special.GetValue(level));
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Session.GetFlag -= OnGetFlag;
        On.Celeste.Session.GetCounter -= OnGetCounter;
        On.Celeste.Session.GetSlider -= OnGetSlider;
        On.Celeste.Session.SetFlag -= OnSetFlag;
        On.Celeste.Session.IncrementCounter -= OnIncrementCounter;
        OnSliderObjectGetValue?.Dispose();
        OnSliderObjectSetValue?.Dispose();
        //Everest.Events.LevelLoader.OnLoadingThread -= OnLevelInit;
    }

    private static bool OnGetFlag(On.Celeste.Session.orig_GetFlag orig, Session self, string flag) {
        if (flag is not null && Engine.Scene is Level level && flags.TryGetValue(flag, out var specialFlag)) return specialFlag.GetValue(level);
        else return orig(self, flag);
    }

    private static int OnGetCounter(On.Celeste.Session.orig_GetCounter orig, Session self, string counter) {
        if (counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) return specialCounter.GetValue(level);
        else return orig(self, counter);
    }

    private static float OnGetSlider(On.Celeste.Session.orig_GetSlider orig, Session self, string slider) {
        if (slider is not null && Engine.Scene is Level level && sliders.TryGetValue(slider, out var specialSlider)) return specialSlider.GetValue(level);
        else return orig(self, slider);
    }

    private static void OnSetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool value) {
        if (flag is not null && Engine.Scene is Level level && flags.TryGetValue(flag, out var specialFlag)) specialFlag.SetValue(level, value);
        else orig(self, flag, value);
    }

    private static void OnSetCounter(On.Celeste.Session.orig_SetCounter orig, Session self, string counter, int value) {
        if (counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) specialCounter.SetValue(level, value);
        else orig(self, counter, value);
    }

    private static void OnIncrementCounter(On.Celeste.Session.orig_IncrementCounter orig, Session self, string counter) {
        if (counter is not null && Engine.Scene is Level level && counters.TryGetValue(counter, out var specialCounter)) specialCounter.SetValue(level, specialCounter.GetValue(level) + 1);
        else orig(self, counter);
    }

    private static float OnSliderObjectGet(Func<Session.Slider, float> orig, Session.Slider self) {
        if (sliders.TryGetValue(self.Name, out var specialSlider))
            return (Engine.Scene is Level level) ? specialSlider.GetValue(level) : 0f;
        return orig(self);
    }

    private static void OnSliderObjectSet(Action<Session.Slider, float> orig, Session.Slider self, float value) {
        if (sliders.TryGetValue(self.Name, out var specialSlider)) {
            if (Engine.Scene is Level level) specialSlider.SetValue(level, value);
        } else orig(self, value);
    }

    [Command("getflag", "Gets the value of a flag.")]
    internal static void CmdGetFlag(string name) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        Engine.Commands.Log($"{name}: {level.Session.GetFlag(name)}");
    }
    [Command("getcounter", "Gets the value of a counter.")]
    internal static void CmdGetCounter(string name) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        Engine.Commands.Log($"{name}: {level.Session.GetCounter(name)}");
    }
    [Command("getslider", "Gets the value of a slider.")]
    internal static void CmdGetSlider(string name) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        Engine.Commands.Log($"{name}: {level.Session.GetSlider(name)}");
    }

    [Command("setflag", "Sets the value of a flag.")]
    internal static void CmdSetFlag(string name, bool value) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        level.Session.SetFlag(name, value);
    }
    [Command("setcounter", "Sets the value of a counter.")]
    internal static void CmdSetCounter(string name, int value) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        level.Session.SetCounter(name, value);
    }
    [Command("setslider", "Sets the value of a slider.")]
    internal static void CmdSetSlider(string name, float value) {
        if (name is null || name == "" || Engine.Scene is not Level level) return;
        FlushSSVStates(level);
        level.Session.SetSlider(name, value);
    }

    [Command("sessionvars", "Shows currently set flags, counters, and sliders. An optional first argument searches for values with a given string in their name.")]
    internal static void ShowValues(string? search = null) {
        if (Engine.Scene is not Level lv) return;
        Session session = lv.Session;
        FlushSSVStates(lv);
        Engine.Commands.Log($"Flags:");
        foreach (string flag in session.Flags)
            if (search is null || flag.Contains(search))
                Engine.Commands.Log($"- {flag}");
        Engine.Commands.Log($"Counters:");
        foreach (Session.Counter counter in session.Counters)
            if (search is null || counter.Key.Contains(search))
                Engine.Commands.Log($"- {counter.Key}: {counter.Value}");
        Engine.Commands.Log($"Sliders:");
        foreach (Session.Slider slider in session.Sliders.Values)
            if (search is null || slider.Name.Contains(search))
                Engine.Commands.Log($"- {slider.Name}: {slider.Value}");
    }
}
