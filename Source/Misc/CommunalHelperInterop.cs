using System;
using Celeste.Mod;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

using Celeste;
using MonoMod.Utils;

#nullable enable

public static class CommunalHelperInterop {
    public static bool Loaded { get; private set; }

    [OnLoad]
    internal static void CheckLoaded() {
        EverestModuleMetadata communalHelper = new() {
          Name = "CommunalHelper",
          Version = new Version(1, 25, 3)
        };

        Loaded = Everest.Loader.DependencyLoaded(communalHelper);
    }

    static bool _GetCanDeployElytra() => Celeste.Mod.CommunalHelper.CommunalHelperModule.Session.CanDeployElytra;
    static void _SetCanDeployElytra(bool value) => Celeste.Mod.CommunalHelper.CommunalHelperModule.Session.CanDeployElytra = value;
    
    public static bool CanDeployElytra {
        get => Loaded && _GetCanDeployElytra();
        set { if (Loaded) _SetCanDeployElytra(value); }
    }
    
    static int _GetElytraState() => Celeste.Mod.CommunalHelper.States.St.Elytra;
    static int _GetDreamTunnelDashState() => Celeste.Mod.CommunalHelper.States.St.DreamTunnelDash;

    public static int? ElytraState => Loaded ? _GetElytraState() : null;
    public static int? DreamTunnelDashState => Loaded ? _GetDreamTunnelDashState() : null;
    
    static int _GetDreamTunnelDashCount() => Celeste.Mod.CommunalHelper.DashStates.DreamTunnelDash.DreamTunnelDashCount;
    static void _SetDreamTunnelDashCount(int value) => Celeste.Mod.CommunalHelper.DashStates.DreamTunnelDash.DreamTunnelDashCount = value;

    public static int DreamTunnelDashCount {
        get => Loaded ? _GetDreamTunnelDashCount() : 0;
        set { if (Loaded) _SetDreamTunnelDashCount(value); }
    }
    
    static bool _GetHasSeekerDash() => Celeste.Mod.CommunalHelper.DashStates.SeekerDash.HasSeekerDash;
    static void _SetHasSeekerDash(bool value) => Celeste.Mod.CommunalHelper.DashStates.SeekerDash.HasSeekerDash = value;
    
    public static bool HasSeekerDash {
        get => Loaded && _GetHasSeekerDash();
        set { if (Loaded) _SetHasSeekerDash(value); }
    }
    
    public static bool IsElytraInfinite(this Player player) => (bool) (DynamicData.For(player).Get("f_Player_elytraIsInfinite") ?? false);
    public static void SetElytraInfinite(this Player player, bool value) => DynamicData.For(player).Set("f_Player_elytraIsInfinite", value);
    
    public static float GetElytraCooldown(this Player player) => (float) (DynamicData.For(player).Get("f_Player_elytraCooldown") ?? 0.0f);
    public static void SetElytraCooldown(this Player player, float value) => DynamicData.For(player).Set("f_Player_elytraCooldown", value);
}
