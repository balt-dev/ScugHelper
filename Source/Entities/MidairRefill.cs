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

#nullable enable
internal static class Ext {
    internal static DynamicData GetData(this Player player) => DynamicData.For(player);
}

[Tracked]
[CustomEntity("ScugHelper/MidairRefill")]
public class MidairRefill : Refill
{
    public MidairRefill(Vector2 position, bool oneUse) : base(position, false, oneUse)
    {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/midairRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/midairRefill/outline"]));
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
    public MidairRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render()
    {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void NewOnPlayer(Player player)
    {
        if (MidairDashCount == 0) {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(NewRefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }
    public IEnumerator NewRefillRoutine(Player player)
    {
        Celeste.Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        MidairDashCount = 1;
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
    public static void LoadHooks() {
        On.Celeste.Refill.OnPlayer += OnPlayerHook;
        On.Celeste.Player.ctor += Player_ctor;
        On.Celeste.Player.DashBegin += Player_DashBegin;
        On.Celeste.Player.CreateTrail += Player_CreateTrail;
        On.Celeste.Player.Update += Player_Update;
        IL.Celeste.Player.DashUpdate += DashUpdateHook;
        IL.Celeste.Player.RedDashUpdate += DashUpdateHook;
        IL.Celeste.Player.NormalUpdate += NormalUpdateHook;
        IL.Celeste.Player.DashUpdate += DashOrRedDashUpdateHook;
        IL.Celeste.Player.RedDashUpdate += DashOrRedDashUpdateHook;
    }

    public static void UnloadHooks() {
        On.Celeste.Refill.OnPlayer -= OnPlayerHook;
        On.Celeste.Player.ctor -= Player_ctor;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
        On.Celeste.Player.CreateTrail -= Player_CreateTrail;
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Player.DashUpdate -= DashUpdateHook;
        IL.Celeste.Player.RedDashUpdate -= DashUpdateHook;
        IL.Celeste.Player.NormalUpdate -= NormalUpdateHook;
        IL.Celeste.Player.DashUpdate -= DashOrRedDashUpdateHook;
        IL.Celeste.Player.RedDashUpdate -= DashOrRedDashUpdateHook;
    }

    private static void OnPlayerHook(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
    {
        if (self is MidairRefill refill)
            refill.NewOnPlayer(player);
        else
            orig(self, player);
    }

    private static bool MidairDashing;
    private static int MidairDashCount;
    private static float MidairDashTimer;

    private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player player, Vector2 position, PlayerSpriteMode spriteMode) {
        orig(player, position, spriteMode);
        MidairDashing = false;
    }

    private static void StartMidairDash(Player player) {
        MidairDashing = true;
        MidairDashTimer = player.GetData().Get<float>("dashAttackTimer");
    }
    private static bool UseMidairDash() {
        if (MidairDashCount > 0) {
            MidairDashCount--;
            return true;
        }
        return false;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        if (UseMidairDash())
            StartMidairDash(self);
    }

    private static void CreateBlueTrail(Player player) {
        Vector2 scale = new(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
        TrailManager.Add(player, scale, Color.Blue);
    }

    private static void Player_CreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player player) {
        if (MidairDashCount > 0 || MidairDashing)
            CreateBlueTrail(player);
        else
            orig(player);
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        float dashAttackTimer = self.GetData().Get<float>("dashAttackTimer");
        if (dashAttackTimer < MidairDashTimer)
            MidairDashTimer = dashAttackTimer;
        else if (MidairDashTimer > 0)
            MidairDashTimer -= Engine.DeltaTime;

        if (MidairDashTimer <= 0f)
            MidairDashing = false;

        if (MidairDashCount > 0 && self.Scene.OnInterval(0.1f))
            CreateBlueTrail(self);
    }

    // For supers and hypers
    private static void DashUpdateHook(ILContext il)
    {
        ILCursor cur = new(il);

        ILLabel labelPastCheck = cur.DefineLabel();

        if (!cur.TryGotoNext(MoveType.AfterLabel,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<Player>("jumpGraceTimer")
        )) throw new InvalidOperationException("Midair refills failed to match IL code for the Dash Update hook.");

        cur.EmitDelegate(static () => MidairDashing);
        cur.Emit(OpCodes.Brtrue, labelPastCheck);

        if (!cur.TryGotoNext(MoveType.After, instr => instr.MatchBleUn(out _)
        )) throw new InvalidOperationException("Midair refills failed to match IL code for the Dash Update hook.");

        cur.MarkLabel(labelPastCheck);
    }

    private static readonly MethodInfo m_SuperWallJump = typeof(Player).GetMethod("SuperWallJump", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo m_WallJump = typeof(Player).GetMethod("WallJump", BindingFlags.NonPublic | BindingFlags.Instance)!;


    private static void DashOrRedDashUpdateHook(ILContext il) {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(
            MoveType.Before,
            static instr => instr.MatchLdarg(0),
            static instr => instr.MatchLdcI4(1),
            static instr => instr.MatchCallvirt(m_SuperWallJump),
            static instr => instr.MatchLdcI4(0),
            static instr => instr.MatchRet())
        ) throw new InvalidOperationException("Midair refills failed to match IL code for the Dash Update hook.");

        // yoink, you'll be needed later
        Instruction brfalse_Continue = cursor.Prev;
        ILLabel @continue = (ILLabel) brfalse_Continue.Operand;

        cursor.GotoNext(MoveType.After, instr => instr.MatchRet());
        ILLabel tryWalllessWallbounce = cursor.MarkLabel();

        // don't cursor.MoveAfterLabels();, else you'll emit IL in the wrong place

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(WalllessWallbounceDashCheck);
        cursor.Emit(OpCodes.Brfalse, @continue);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ret);

        // remember to fix the label! else our patch will get skipped
        brfalse_Continue.Operand = tryWalllessWallbounce;
    }

    private static void NormalUpdateHook(ILContext il) {
        ILCursor cursor = new(il);

        // we want to favor vanilla wallbounce behavior, so we append our own to the end
        if (!cursor.TryGotoNext(MoveType.After,
            static instr => instr.MatchLdarg(0),
            static instr => instr.MatchLdcI4(1),
            static instr => instr.MatchCallvirt(m_WallJump))
        ) throw new InvalidOperationException("Midair refills failed to match IL code for the Normal Update hook.");

        // yoink, you'll be needed later
        ILLabel? cont = cursor.Next?.Operand as ILLabel;

        cursor.Index++;

        // important to cursor.MoveAfterLabels, else the labels won't point to our patch
        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, (byte) 14);
        cursor.EmitDelegate(WalllessWallbounceNormalCheck);
        cursor.Emit(OpCodes.Brtrue_S, cont);
    }

    private static bool WalllessWallbounceDashCheck(Player player) {
        if (MidairDashing)
            DoWallbounce(player);
        return MidairDashing;
    }

    private static bool WalllessWallbounceNormalCheck(Player player, bool canUnDuck) {
        bool canWallbounce
            = canUnDuck
            && player.DashAttacking
            && player.SuperWallJumpAngleCheck
            && MidairDashing;

        if (canWallbounce)
            DoWallbounce(player);
        return canWallbounce;
    }

    private static void DoWallbounce(Player player) {
        player.SuperWallJump((int) player.Facing);
    }
}
#nullable restore
