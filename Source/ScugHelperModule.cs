using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.ScugHelper;

#nullable enable

public class ScugHelperModule : EverestModule
{
    public static ScugHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(ScugHelperModuleSettings);
    public static ScugHelperModuleSettings Settings => (ScugHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(ScugHelperModuleSession);
    public static ScugHelperModuleSession Session => (ScugHelperModuleSession)Instance._Session;

    public ScugHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ScugHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ScugHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        // TODO: apply any hooks that should always be active
        typeof(FrostHelperImports).ModInterop();
        typeof(GravityHelperImports).ModInterop();
        LifecycleMethods.OnLoad();
        On.Celeste.PlayerSeeker.OnCollide += OnPlayerSeekerCollideHook;
        On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int += OnSquishWiggle;
    }

    public override void Unload() {
        // TODO: unapply any hooks applied in Load()
        LifecycleMethods.OnUnload();
        On.Celeste.PlayerSeeker.OnCollide -= OnPlayerSeekerCollideHook;
        On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int -= OnSquishWiggle;
    }

    private static void OnPlayerSeekerCollideHook(On.Celeste.PlayerSeeker.orig_OnCollide orig, PlayerSeeker self, CollisionData data) {
        orig(self, data);
        if (Settings.PlayerSeekerDashSwitchFix && data.Hit is DashSwitch dashSwitch) {
            Logger.Info(nameof(ScugHelperModule), $"PlayerSeeker collided: ${dashSwitch.pressed} ${dashSwitch.pressDirection} ${self.dashDirection}");
            dashSwitch.OnDashed(null, Vector2.UnitX * Math.Sign(self.dashDirection.X));
            dashSwitch.OnDashed(null, Vector2.UnitY * Math.Sign(self.dashDirection.Y));
        }
    }

    private static bool OnSquishWiggle(On.Celeste.Actor.orig_TrySquishWiggle_CollisionData_int_int orig, Actor self, CollisionData data, int wiggleX, int wiggleY) {
        if (KillingSeeker) return false;
        return orig(self, data, wiggleX, wiggleY);
    }


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
            Logger.Warn(nameof(ScugHelperModule), $"SID {data.Name} of entity with ID {data.ID} does not correspond to any known types.");
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

    internal static Effect OutlineFX;

    public override void LoadContent(bool firstLoad) {
        base.LoadContent(firstLoad);

        OutlineFX = new Effect(Engine.Graphics.GraphicsDevice, Everest.Content.Get($"Shaders/outline.cso", true).Data);
    }
}
