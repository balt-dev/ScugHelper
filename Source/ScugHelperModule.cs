using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;

namespace Celeste.Mod.ScugHelper;

public class ScugHelperModule : EverestModule
{
    internal bool ForceRenderDebug = false;
    internal static readonly List<Action> LoadHooks;
    internal static readonly List<Action> UnloadHooks;

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
        typeof(FrostHelperImports).ModInterop();
        LifecycleMethods.OnLoad();
        On.Celeste.PlayerSeeker.OnCollide += OnPlayerSeekerCollideHook;
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
        LifecycleMethods.OnUnload();
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
