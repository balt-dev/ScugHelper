#nullable enable

using System;

namespace Celeste.Mod.ScugHelper.SpecialSessionVariables;

internal static class CommunalHelperSSVInterop
{
    internal static void LoadVariables() {
        SSV.flags["ScugHelper.CommunalHelper.Enabled"] = new CommunalHelperEnabled();
        SSV.flags.Add("ScugHelper.CommunalHelper.CanDeployElytra", new CanDeployElytraFlag());
        SSV.flags.Add("ScugHelper.CommunalHelper.IsElytraInfinite", new IsElytraInfiniteFlag());
        SSV.flags.Add("ScugHelper.CommunalHelper.HasSeekerDash", new HasSeekerDashFlag());
        SSV.counters.Add("ScugHelper.CommunalHelper.StElytra", new StElytraCounter());
        SSV.counters.Add("ScugHelper.CommunalHelper.StDreamTunnelDash", new StDreamTunnelDashCounter());
        SSV.counters.Add("ScugHelper.CommunalHelper.DreamTunnelDashCount", new DreamTunnelDashCountCounter());
        SSV.sliders.Add("ScugHelper.CommunalHelper.ElytraCooldown", new ElytraCooldownSlider());
    }

    internal class CommunalHelperEnabled : SpecialFlag {
        public override bool GetValue(Level level) => CommunalHelperInterop.Loaded;
    }
}

internal class CanDeployElytraFlag : SpecialFlag {
    public override bool GetValue(Level level) => CommunalHelperInterop.CanDeployElytra;
    public override void SetValue(Level level, bool value) => CommunalHelperInterop.CanDeployElytra = value;
}

internal class IsElytraInfiniteFlag : SpecialFlag {
    public override bool GetValue(Level level) => level.GetPlayer() is Player player && player.IsElytraInfinite();
    public override void SetValue(Level level, bool value) { if (level.GetPlayer() is Player player) player.SetElytraInfinite(value); }
}

internal class HasSeekerDashFlag : SpecialFlag {
    public override bool GetValue(Level level) => CommunalHelperInterop.HasSeekerDash;
    public override void SetValue(Level level, bool value) => CommunalHelperInterop.HasSeekerDash = value;
}

internal class StElytraCounter : SpecialCounter {
    public override int GetValue(Level level) => CommunalHelperInterop.ElytraState ?? -1;
}

internal class StDreamTunnelDashCounter : SpecialCounter {
    public override int GetValue(Level level) => CommunalHelperInterop.DreamTunnelDashState ?? -1;
}

internal class DreamTunnelDashCountCounter : SpecialCounter {
    public override int GetValue(Level level) => CommunalHelperInterop.DreamTunnelDashCount;
    public override void SetValue(Level level, int value) => CommunalHelperInterop.DreamTunnelDashCount = value;
}

internal class ElytraCooldownSlider : SpecialSlider {
    public override float GetValue(Level level) => level.GetPlayer() is Player player ? player.GetElytraCooldown() : 0.0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.SetElytraCooldown(value); }
}