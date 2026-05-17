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
    public OverchargeRefill(Vector2 position, bool oneUse) : base(position, false, oneUse)
    {
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
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }
    public OverchargeRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render()
    {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player)
    {
        if (OverchargeDashCount == 0) {
            Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(NewRefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }
    public IEnumerator NewRefillRoutine(Player player)
    {
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
    private static ILHook? PlayerDashCoroHook;

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Player.CreateTrail += Player_CreateTrail;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        On.Celeste.Player.BeforeUpTransition += Player_BeforeUpTransition;
        On.Celeste.Level.Reload += Level_Reload;
        On.Celeste.LevelLoader.StartLevel += LevelLoader_StartLevel;
        using (new DetourConfigContext(
            new DetourConfig("ScugHelper").WithPriority(1000000000)
        ).Use()) {
            PlayerDashCoroHook = new(PlayerDashCoro, OnPlayerDashCoro);
        }
    }

    private static void Player_BeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self)
    {
        var oldYSpeed = self.Speed.Y;
        orig(self);
        if (OverchargeDashCount > 0) {
            self.Speed.Y = MathF.Min(oldYSpeed, self.Speed.Y);
            self.dashCooldownTimer = 0f;
        }
    }

    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Player.CreateTrail -= Player_CreateTrail;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        On.Celeste.Level.Reload -= Level_Reload;
        On.Celeste.LevelLoader.StartLevel -= LevelLoader_StartLevel;
        PlayerDashCoroHook?.Dispose();
    }

    public static int OverchargeDashCount { get; internal set; }

    private static bool HasOverchargeDash() => OverchargeDashCount > 0;

    internal static readonly Color TrailColor = Calc.HexToColor("a5adff");

    private static void CreateTrail(Player player) {
        Vector2 scale = new(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
        TrailManager.Add(player, scale, TrailColor);
    }

    private static void Player_CreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player player)
    {
        if (OverchargeDashCount > 0)
            CreateTrail(player);
        else
            orig(player);
    }


    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        var oldSpeedX = MathF.Abs(self.Speed.X);
        orig(self);
        var newSpeedX = MathF.Abs(self.Speed.X);
        if (OverchargeDashCount > 0)
        {
            self.Speed.X = MathF.Max(oldSpeedX, newSpeedX) * (float)self.Facing * 1.2f;
            OverchargeDashCount--;
        }
    }
    
    private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        var oldSpeedY = self.Speed.Y;
        orig(self, dir);
        if (OverchargeDashCount > 0)
            self.Speed.Y = MathF.Min(oldSpeedY, self.Speed.Y) * 1.2f; // -Y = up
    }

    public static readonly float LoseOverchargeTime = 0.3f;

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (ScugHelperModule.Settings.AlwaysOvercharges)
            OverchargeDashCount = 1;

        if (OverchargeDashCount > 0 && self.Scene.OnInterval(0.07f))
            CreateTrail(self);
    }

    private static void Level_Reload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        OverchargeDashCount = 0;
        orig(self);
    }

    private static void LevelLoader_StartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self)
    {
        OverchargeDashCount = 0;
        orig(self);
    }

    private static void OnPlayerDashCoro(ILContext il) {
        ILCursor cur = new(il);
        ILLabel label = null!;

        if (!cur.TryGotoNextBestFit(MoveType.Before, 16,
            static instr => instr.MatchLdloc1(),
            static instr => instr.MatchLdloc3(),
            static instr => instr.MatchStfld<Player>(nameof(Player.Speed))
        )) {Logger.Warn(nameof(ScugHelperModule), "Failed to hook player dash coroutine for overcharge refills! (ldloc1, ldloc3, stfld Player Speed)"); return;}
        cur.MoveAfterLabels();
        cur.EmitLdloc1();
        cur.EmitLdloc3();
        static Vector2 MultiplyOvercharge(Player self, Vector2 speed) {
            if (OverchargeDashCount > 0) {
                speed = Math.Max(self.beforeDashSpeed.Length(), speed.Length()) * speed.SafeNormalize() * 1.1f;
            }
            return speed;
        }
        cur.EmitDelegate(MultiplyOvercharge);
        cur.EmitStloc3();

        if (!cur.TryGotoNext(MoveType.After, static instr => instr.MatchLdfld<Player>(nameof(Player.DashDir)))) {Logger.Warn(nameof(ScugHelperModule), "Failed to hook player dash coroutine for overcharge refills! (ldfld Player DashDir)"); return;}
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            static instr => instr.MatchCall<Vector2>("op_Multiply"),
            static instr => instr.MatchStfld<Player>(nameof(Player.Speed))
        )) {Logger.Warn(nameof(ScugHelperModule), "Failed to hook player dash coroutine for overcharge refills! (call Vector2 op_Multiply, stfld Player Speed)"); return;}
        if (!cur.TryGotoPrev(MoveType.After,
            instr => instr.MatchBgtUn(out label)
        )) {Logger.Warn(nameof(ScugHelperModule), "Failed to hook player dash coroutine for overcharge refills! (bgt.un)"); return;}
        cur.MoveAfterLabels();
        cur.EmitDelegate(HasOverchargeDash);
        cur.EmitBrtrue(label);
    }

    [Command("giveovercharge", "Gives the player an overcharge dash.")]
    private static void GiveOvercharge() {
        OverchargeDashCount = 1;
    }
}
#nullable restore
