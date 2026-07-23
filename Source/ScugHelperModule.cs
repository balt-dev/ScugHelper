using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Celeste.Mod.Registry;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.ScugHelper;

#nullable enable

public class ScugHelperModule : EverestModule
{
    public static ScugHelperModule Instance { get; private set; } = null!;

    public override Type SettingsType => typeof(ScugHelperModuleSettings);
    public static ScugHelperModuleSettings Settings => (ScugHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(ScugHelperModuleSession);
    public static ScugHelperModuleSession Session => (ScugHelperModuleSession)Instance._Session;

    public ScugHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ScugHelper), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ScugHelper), LogLevel.Info);
#endif
    }

    static Exception? QueuedException;

    public override void Load() {
        On.Celeste.Overworld.Update += OnOverworldUpdate;
        On.Celeste.Level.Update += OnLevelUpdate;
        try {
            typeof(FrostHelperImports).ModInterop();
            typeof(GravityHelperImports).ModInterop();
            typeof(ExtendedVariantModeImports).ModInterop();
            typeof(MotionSmoothingImportHandler).ModInterop();
            LifecycleMethods.OnLoad();
            On.Celeste.PlayerSeeker.OnCollide += OnPlayerSeekerCollideHook;
            On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int += OnSquishWiggle;
        } catch (Exception e) {
            QueuedException = e;
        }
    }
    
    private static void OnOverworldUpdate(On.Celeste.Overworld.orig_Update orig, Overworld self) {
        if (QueuedException is {} exc) { QueuedException = null; throw exc; }
        else orig(self);
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        if (QueuedException is {} exc) { QueuedException = null; throw exc; }
        else orig(self);
    }

    public override void Unload() {
        On.Celeste.Overworld.Update -= OnOverworldUpdate;
        On.Celeste.Level.Update -= OnLevelUpdate;
        LifecycleMethods.OnUnload();
        On.Celeste.PlayerSeeker.OnCollide -= OnPlayerSeekerCollideHook;
        On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int -= OnSquishWiggle;
    }

    private static void OnPlayerSeekerCollideHook(On.Celeste.PlayerSeeker.orig_OnCollide orig, PlayerSeeker self, CollisionData data) {
        orig(self, data);
        if (Settings.PlayerSeekerDashSwitchFix && data.Hit is DashSwitch dashSwitch) {
            Logger.Log(nameof(ScugHelper), $"PlayerSeeker collided: ${dashSwitch.pressed} ${dashSwitch.pressDirection} ${self.dashDirection}");
            dashSwitch.OnDashed(null, Vector2.UnitX * Math.Sign(self.dashDirection.X));
            dashSwitch.OnDashed(null, Vector2.UnitY * Math.Sign(self.dashDirection.Y));
        }
    }

    private static bool OnSquishWiggle(On.Celeste.Actor.orig_TrySquishWiggle_CollisionData_int_int orig, Actor self, CollisionData data, int wiggleX, int wiggleY) {
        if (KillingSeeker) return false;
        return orig(self, data, wiggleX, wiggleY);
    }

    internal static string DebugText = "<unset>";

    static bool KillingSeeker;

    public static void KillSeeker(Seeker self) {
        KillingSeeker = true;
        try {
            var solid = new Solid(Vector2.Zero, 0, 0, false);
            self.SquishCallback(new CollisionData() {
                Direction = Vector2.Zero,
                Moved = Vector2.Zero,
                TargetPosition = self.Position,
                Hit = solid,
                Pusher = solid
            });
        } finally {
            KillingSeeker = false;
        }
    }

    internal static Effect? OutlineFX;
    internal static Effect? SeekerBarrierFX;
    internal static Effect? OutlineWithBaseFX;
    internal static Effect? HallOfMirrorsFX;
    internal static Effect? BalatroFX;
    internal static Effect? PixelDistortionFX;
    internal static Effect? BlankFX;
    internal static bool PreventDeath;

    public override void LoadContent(bool firstLoad) {
        base.LoadContent(firstLoad);
        RunnableLoadContent();
    }
    [Command("scug_reloadshaders", "Reloads ScugHelper shaders. Development command, probably does nothing if you're not the developer.")]
    static void RunnableLoadContent() {
        SeekerBarrierFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/seekerBarrier.cso", true).Data);
        OutlineFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/outline.cso", true).Data);
        OutlineWithBaseFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/outlineWithBase.cso", true).Data);
        HallOfMirrorsFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/hallOfMirrors.cso", true).Data);
        BalatroFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/balatro.cso", true).Data);
        PixelDistortionFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/pixelDistort.cso", true).Data);
        BlankFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Effects/ScugHelper/blank.cso", true).Data);
    }
}