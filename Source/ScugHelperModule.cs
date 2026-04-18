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
        Entities.PinballBooster.LoadHooks();
        Entities.Gripwall.LoadHooks();
        Entities.BrassBerry.LoadHooks();
        Entities.DebugViewTrigger.LoadHooks();
        Entities.DashlessHeartGem.LoadHooks();
        Entities.RecoilBumper.LoadHooks();
        Entities.SquareCrystal.LoadHooks();
        Entities.ICustomRefill.LoadHooks();
        Entities.MidairRefill.LoadHooks();
        Entities.GroundedRefill.LoadHooks();
        Entities.FreezeRefill.LoadHooks();
        Entities.TungstenCube.LoadHooks();
        Entities.FastfallBlock.LoadHooks();
        Entities.Cycler.LoadHooks();
        Entities.Actions.ActionManager.LoadHooks();
        On.Celeste.PlayerSeeker.OnCollide += OnPlayerSeekerCollideHook;
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
        Entities.PinballBooster.UnloadHooks();
        Entities.Gripwall.UnloadHooks();
        Entities.BrassBerry.UnloadHooks();
        Entities.DebugViewTrigger.UnloadHooks();
        Entities.DashlessHeartGem.UnloadHooks();
        Entities.RecoilBumper.UnloadHooks();
        Entities.SquareCrystal.UnloadHooks();
        Entities.ICustomRefill.UnloadHooks();
        Entities.MidairRefill.UnloadHooks();
        Entities.GroundedRefill.UnloadHooks();
        Entities.FreezeRefill.UnloadHooks();
        Entities.TungstenCube.UnloadHooks();
        Entities.FastfallBlock.UnloadHooks();
        Entities.Cycler.UnloadHooks();
        Entities.Actions.ActionManager.UnloadHooks();
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
