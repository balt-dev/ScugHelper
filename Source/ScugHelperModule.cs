using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper;

public class ScugHelperModule : EverestModule
{
    internal bool ForceRenderDebug = false;

    public static ScugHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(ScugHelperModuleSettings);
    public static ScugHelperModuleSettings Settings => (ScugHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(ScugHelperModuleSession);
    public static ScugHelperModuleSession Session => (ScugHelperModuleSession) Instance._Session;

    public ScugHelperModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ScugHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ScugHelperModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        // TODO: apply any hooks that should always be active
        PinballBooster.LoadHooks();
        BoosterField.LoadHooks();
        Gripwall.LoadHooks();
        BrassBerry.LoadHooks();
        DebugViewTrigger.LoadHooks();
        DashlessHeartGem.LoadHooks();
        RecoilBumper.LoadHooks();
        SquareCrystal.LoadHooks();
        On.Celeste.PlayerSeeker.OnCollide += OnPlayerSeekerCollideHook;
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
        PinballBooster.UnloadHooks();
        BoosterField.UnloadHooks();
        Gripwall.UnloadHooks();
        BrassBerry.UnloadHooks();
        DebugViewTrigger.UnloadHooks();
        DashlessHeartGem.UnloadHooks();
        RecoilBumper.UnloadHooks();
        SquareCrystal.UnloadHooks();
        On.Celeste.PlayerSeeker.OnCollide -= OnPlayerSeekerCollideHook;
    }

    private static void OnPlayerSeekerCollideHook(On.Celeste.PlayerSeeker.orig_OnCollide orig, PlayerSeeker self, CollisionData data)
    {
        orig(self, data);
        if (Settings.PlayerSeekerDashSwitchFix && data.Hit is DashSwitch dashSwitch) {
            Logger.Info(nameof(ScugHelperModule), $"PlayerSeeker collided: ${dashSwitch.pressed} ${dashSwitch.pressDirection} ${self.dashDirection}");
            dashSwitch.OnDashed(null, Vector2.UnitX * Math.Sign(self.dashDirection.X));
            dashSwitch.OnDashed(null, Vector2.UnitY * Math.Sign(self.dashDirection.Y));
        }
    }
}
