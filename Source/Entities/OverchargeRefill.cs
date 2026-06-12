using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;
using System.Collections;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Reflection;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.RuntimeDetour;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/OverchargeRefill")]
public class OverchargeRefill : Refill, ICustomRefill
{
    public OverchargeRefill(Vector2 position, bool oneUse) : base(position, false, oneUse) {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/ScugHelper/overchargeRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/ScugHelper/overchargeRefill/outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        outline.Visible = false;
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
    }
    public override void Added(Scene scene) {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }
    public OverchargeRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render() {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player) {
        if (OverchargeDashCount == 0) {
            Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(NewRefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }
    public IEnumerator NewRefillRoutine(Player player) {
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        OverchargeDashCount = 1;
        player.RefillDash();
        player.RefillStamina();
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }


    private static readonly MethodInfo PlayerDashCoro = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!.GetStateMachineTarget()!;
    private static ILHook? ILPlayerDashCoroHook;

    private static ILHook? ILCommunalHelperPlayerDreamTunnelDashBeginHook;

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Player.CreateTrail += OnPlayerCreateTrail;
        On.Celeste.Player.Update += OnPlayerUpdate;
        On.Celeste.Player.SuperJump += OnPlayerSuperJump;
        On.Celeste.Player.SuperWallJump += OnPlayerSuperWallJump;
        On.Celeste.Player.BeforeUpTransition += OnPlayerBeforeUpTransition;
        On.Celeste.Player.BeforeSideTransition += OnPlayerBeforeSideTransition;
        On.Celeste.Player.BeforeDownTransition += OnPlayerBeforeDownTransition;
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStartLevel;
        IL.Celeste.Player.DreamDashBegin += ILPlayerDreamDashBegin;

        using (new DetourConfigContext(
            new DetourConfig("ScugHelper").WithPriority(1000000000)
        ).Use()) {
            ILPlayerDashCoroHook = new(PlayerDashCoro, ILPlayerDashCoro);
        }

        CommunalHelperInterop.CheckLoaded();
        if (CommunalHelperInterop.Loaded) {
            using (new DetourConfigContext(
                new DetourConfig("ScugHelper").WithPriority(-1000000000)
            ).Use()) {
                Type? maybeDreamTunnelDashType = Type.GetType("Celeste.Mod.CommunalHelper.States.DreamTunnelDash, CommunalHelper");
                if (maybeDreamTunnelDashType is Type dreamTunnelDashType) {
                    // evil crossmod hook
                    MethodInfo? maybeInfo = dreamTunnelDashType
                        .GetMethod("DreamTunnelDashBegin", BindingFlags.Public | BindingFlags.Static);
                    if (maybeInfo is MethodInfo info)
                        ILCommunalHelperPlayerDreamTunnelDashBeginHook = new(info, ILCommunalHelperPlayerDreamTunnelDashBegin);
                    else
                        throw new Utils.HookException("Failed to load method info for dream tunnel dash begin for overcharge refills!");
                } else
                    throw new Utils.HookException("Failed to load dream tunnel dash type for overcharge refills!");
            }
        } else {
            Logger.Log(nameof(ScugHelper), "CommunalHelper isn't loaded! Skipping CommunalHelper overcharge hooks...");
        }
    }

    private static void ILCommunalHelperPlayerDreamTunnelDashBegin(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.AfterLabel,
            static instr => instr.MatchStfld<Player>(nameof(Player.Speed))
        )) throw new Utils.HookException("Failed to hook player dream tunnel dash begin for overcharge refills!");

        static Vector2 GetNewSpeed(Vector2 origSpeed, Player player)
            => OverchargeDashCount <= 0 ? origSpeed : player.DashDir * MathF.Max(player.Speed.Length(), origSpeed.Length());
        cur.EmitLdarg0();
        cur.EmitDelegate(GetNewSpeed);
    }

    public static bool HasOvercharge =>
        ScugHelperModule.Settings.AlwaysOvercharges || OverchargeDashCount > 0;
    public static void ConsumeOvercharge() {
        if (!ScugHelperModule.Settings.AlwaysOvercharges) OverchargeDashCount--;
    }

    private static void OnPlayerBeforeDownTransition(On.Celeste.Player.orig_BeforeDownTransition orig, Player self) {
        orig(self);
        if (HasOvercharge) {
            self.dashCooldownTimer = 0f;
        }
    }

    private static void OnPlayerBeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self) {
        var oldYSpeed = self.Speed.Y;
        orig(self);
        if (HasOvercharge) {
            self.Speed.Y = MathF.Min(oldYSpeed, self.Speed.Y);
            self.dashCooldownTimer = 0f;
        }
    }

    private static void OnPlayerBeforeSideTransition(On.Celeste.Player.orig_BeforeSideTransition orig, Player self) {
        orig(self);
        if (HasOvercharge) {
            self.dashCooldownTimer = 0f;
        }
    }

    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Player.CreateTrail -= OnPlayerCreateTrail;
        On.Celeste.Player.Update -= OnPlayerUpdate;
        On.Celeste.Player.SuperJump -= OnPlayerSuperJump;
        On.Celeste.Player.SuperWallJump -= OnPlayerSuperWallJump;
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStartLevel;
        ILPlayerDashCoroHook?.Dispose();
    }

    public static int OverchargeDashCount { get; internal set; }

    private static bool HasOverchargeDash() => HasOvercharge;

    internal static readonly Color TrailColor = Calc.HexToColor("a5adff");

    private static void CreateTrail(Player player) {
        Vector2 scale = new(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y * (GravityHelperImports.PlayerInverted() ? -1 : 1));
        TrailManager.Add(player, scale, TrailColor);
    }

    private static void OnPlayerCreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player player) {
        if (HasOvercharge)
            CreateTrail(player);
        else
            orig(player);
    }


    private static void OnPlayerSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
        var retainedSpeedX = MathF.Abs(self.wallSpeedRetentionTimer > 0 ? self.wallSpeedRetained : 0f);
        var beforeDashSpeedX = MathF.Abs(self.beforeDashSpeed.X);
        var oldSpeedX = MathF.Abs(self.Speed.X);
        orig(self);
        var newSpeedX = MathF.Abs(self.Speed.X);
        if (HasOvercharge) {
            self.Speed.X = MathF.Max(MathF.Max(oldSpeedX, newSpeedX), MathF.Max(retainedSpeedX, beforeDashSpeedX)) * (float)self.Facing * 1.2f;
            ConsumeOvercharge();
        }
    }

    private static void OnPlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
        var oldSpeedY = self.Speed.Y;
        orig(self, dir);
        if (HasOvercharge && self.level.Session.GetFlag("ScugHelper.EnableSillyOverchargeBehavior"))
            self.Speed.Y = MathF.Min(oldSpeedY, self.Speed.Y) * 1.2f; // -Y = up
    }

    public static readonly float LoseOverchargeTime = 0.3f;

    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        orig(self);

        if (HasOvercharge && self.Scene.OnInterval(0.07f))
            CreateTrail(self);
    }

    private static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
        OverchargeDashCount = 0;
        orig(self);
    }

    private static void OnLevelLoaderStartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
        OverchargeDashCount = 0;
        orig(self);
    }

    private static void ILPlayerDashCoro(ILContext il) {
        ILCursor cur = new(il);
        ILLabel? label = null;

        if (!cur.TryGotoNextBestFit(MoveType.Before, 16,
            static instr => instr.MatchLdloc1(),
            static instr => instr.MatchLdloc3(),
            static instr => instr.MatchStfld<Player>(nameof(Player.Speed))
        )) {Logger.Warn(nameof(ScugHelper), "Failed to hook player dash coroutine for overcharge refills! (ldloc1, ldloc3, stfld Player Speed)"); return;}
        cur.MoveAfterLabels();
        cur.EmitLdloc1();
        cur.EmitLdloc3();
        static Vector2 MultiplyOvercharge(Player self, Vector2 speed) {
            Vector2 playerSpeed = speed;
            if (HasOvercharge) {
                if (self.level.Session.GetFlag("ScugHelper.EnableSillyOverchargeBehavior")) {
                    if (self.level.Session.GetFlag("ScugHelper.LegacyOvercharge"))
                        return Math.Max(self.beforeDashSpeed.Length(), speed.Length()) * speed.SafeNormalize() * 1.1f;
                    speed.Y = (Math.Max(self.beforeDashSpeed.Length(), playerSpeed.Length()) * playerSpeed.SafeNormalize() * 1.1f).Y;
                }
                speed.X = Math.Max(Math.Abs(self.beforeDashSpeed.X), Math.Abs(playerSpeed.X)) * Math.Sign(playerSpeed.X) * 1.1f;
            }
            return speed;
        }
        cur.EmitDelegate(MultiplyOvercharge);
        cur.EmitStloc3();

        if (!cur.TryGotoNext(MoveType.After, static instr => instr.MatchLdfld<Player>(nameof(Player.DashDir)))) {Logger.Warn(nameof(ScugHelper), "Failed to hook player dash coroutine for overcharge refills! (ldfld Player DashDir)"); return;}
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            static instr => instr.MatchCall<Vector2>("op_Multiply"),
            static instr => instr.MatchStfld<Player>(nameof(Player.Speed))
        )) throw new Utils.HookException("Failed to hook player dash coroutine for overcharge refills! (call Vector2 op_Multiply, stfld Player Speed)");
        if (!cur.TryGotoPrev(MoveType.After,
            instr => instr.MatchBgtUn(out label)
        )) throw new Utils.HookException("Failed to hook player dash coroutine for overcharge refills! (bgt.un)");
        cur.MoveAfterLabels();
        cur.EmitDelegate(HasOverchargeDash);
        cur.EmitBrtrue(label!);
    }

    private static void ILPlayerDreamDashBegin(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            static instr => instr.MatchLdcR4(240f),
            static instr => instr.MatchCall<Vector2>("op_Multiply")
        )) throw new Utils.HookException("Failed to hook player dream dash begin for overcharge refills!");
        static Vector2 GetNewSpeed(Vector2 origSpeed, Player player)
            => OverchargeDashCount <= 0 ? origSpeed : player.DashDir * MathF.Max(player.Speed.Length(), origSpeed.Length());
        cur.EmitLdarg0();
        cur.EmitDelegate(GetNewSpeed);
    }

    [Command("giveovercharge", "Gives the player an overcharge dash.")]
    private static void GiveOvercharge() {
        OverchargeDashCount = 1;
    }
}
#nullable restore
