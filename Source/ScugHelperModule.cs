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
        On.Celeste.Overworld.Begin += OnOverworldBegin;
        On.Celeste.Level.Begin += OnLevelBegin;
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

    private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self) {
        if (QueuedException is {} exc) { QueuedException = null; CriticalErrorHandler.HandleCriticalError(ExceptionDispatchInfo.Capture(exc), CriticalErrorHandler.DisplayState.CleanScene); }
        else orig(self);
    }

    private static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self) {
        if (QueuedException is {} exc) { QueuedException = null; CriticalErrorHandler.HandleCriticalError(ExceptionDispatchInfo.Capture(exc), CriticalErrorHandler.DisplayState.CleanScene); }
        else orig(self);
    }    
    
    private static void OnOverworldUpdate(On.Celeste.Overworld.orig_Update orig, Overworld self) {
        if (QueuedException is {} exc) { QueuedException = null; CriticalErrorHandler.HandleCriticalError(ExceptionDispatchInfo.Capture(exc), CriticalErrorHandler.DisplayState.CleanScene); }
        else orig(self);
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        if (QueuedException is {} exc) { QueuedException = null; CriticalErrorHandler.HandleCriticalError(ExceptionDispatchInfo.Capture(exc), CriticalErrorHandler.DisplayState.CleanScene); }
        else orig(self);
    }

    public override void Unload() {
        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        On.Celeste.Level.Begin -= OnLevelBegin;
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
        var solid = new Solid(Vector2.Zero, 0, 0, false);
        self.SquishCallback(new CollisionData() {
            Direction = Vector2.Zero,
            Moved = Vector2.Zero,
            TargetPosition = self.Position,
            Hit = solid,
            Pusher = solid
        });
        KillingSeeker = false;
    }

    static readonly Dictionary<string, Type?> TypeCache = [];
    internal static Type? GetTypeOfEntity(EntityData data) {
        if (TypeCache.TryGetValue(data.Name, out var res)) return res;
        var type = EntityRegistry.GetKnownTypesFromSid(data.Name).AsEnumerable().FirstOrDefault((Type?)null);
        if (type is not Type ty)
            Logger.Warn(nameof(ScugHelper), $"SID {data.Name} of entity with ID {data.ID} does not correspond to any known types.");
        TypeCache[data.Name] = type;
        return type;
    }

    static readonly Dictionary<Type, IReadOnlySet<string>> NameCache = [];
    internal static IReadOnlySet<string> GetNamesOfEntity(Entity entity) {
        var type = entity.GetType();
        if (NameCache.TryGetValue(type, out var res)) return res;
        var sids = EntityRegistry.GetKnownSidsFromType(entity.GetType());
        NameCache[type] = sids;
        return sids;
    }

    internal static Effect? OutlineFX;
    internal static Effect? SeekerBarrierFX;
    internal static Effect? OutlineWithBaseFX;
    internal static Effect? HallOfMirrorsFX;
    internal static Effect? BalatroFX;
    internal static Effect? PixelDistortionFX;

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
    }
}