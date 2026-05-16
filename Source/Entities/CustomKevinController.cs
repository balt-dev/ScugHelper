using System;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/CustomKevinController")]
public class CustomKevinController(EntityData data, Vector2 _) : Entity()
{
    public readonly bool HitDashColliders = data.Bool("HitDashColliders");
    public readonly bool HitTouchSwitches = data.Bool("HitTouchSwitches");
    public readonly bool HitByHoldables = data.Bool("HitByHoldables");
    public readonly float Speed = data.Float("Speed", CrushBlock.CrushSpeed);
    public readonly float Acceleration = data.Float("Acceleration", CrushBlock.CrushAccel);
    public readonly float ReturnSpeed = data.Float("ReturnSpeed", CrushBlock.ReturnSpeed);
    public readonly float ReturnAcceleration = data.Float("ReturnAcceleration", CrushBlock.ReturnAccel);

    private static readonly MethodInfo CrushBlockAttackCoro = typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!.GetStateMachineTarget()!;
    private static ILHook? CrushBlockAttackCoroHook;
    
    [OnLoad]
    internal static void LoadHooks()
    {
        On.Celeste.CrushBlock.Update += OnUpdate;
        IL.Celeste.CrushBlock.MoveHCheck += ILMoveHCheck;
        IL.Celeste.CrushBlock.MoveVCheck += ILMoveVCheck;
        On.Celeste.Glider.OnCollideH += OnGliderCollideH;
        On.Celeste.TheoCrystal.OnCollideH += OnTheoCrystalCollideH;
        CrushBlockAttackCoroHook = new(CrushBlockAttackCoro, ILCrushBlockAttackCoro);
    }

    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Celeste.CrushBlock.Update -= OnUpdate;
        IL.Celeste.CrushBlock.MoveHCheck -= ILMoveHCheck;
        IL.Celeste.CrushBlock.MoveVCheck -= ILMoveVCheck;
        On.Celeste.Glider.OnCollideH -= OnGliderCollideH;
        On.Celeste.TheoCrystal.OnCollideH -= OnTheoCrystalCollideH;
        CrushBlockAttackCoroHook?.Dispose();
    }

    private static void OnUpdate(On.Celeste.CrushBlock.orig_Update orig, CrushBlock self)
    {
        orig(self);
        if (self.Scene.Tracker.GetEntity<CustomKevinController>() is not CustomKevinController ctrl) return;
        if (!ctrl.HitTouchSwitches) return;
        foreach (TouchSwitch touchSwitch in self.CollideAll<TouchSwitch>())
            touchSwitch.TurnOn();
    }

    private static CrushBlock? SmuggledKevin;
    static void SmuggleKevin(CrushBlock block) => SmuggledKevin = block;
    
    static Action<Vector2, Vector2, Platform> LoadKevinCollide() => OnKevinCollide;
    private static void OnKevinCollide(Vector2 direction, Vector2 delta, Platform hit)
    {
        if (SmuggledKevin is not CrushBlock self) return;
        if (self.Scene.Tracker.GetEntity<CustomKevinController>() is not CustomKevinController ctrl) return;
        if (ctrl.HitDashColliders && hit.OnDashCollide is not null) {
            Player player = new(Vector2.Zero, PlayerSpriteMode.Playback);
            var res = hit.OnDashCollide(player, direction);
            switch (res) {
                case DashCollisionResults.Rebound:
                case DashCollisionResults.Bounce:
                    if (!self.CanActivate(-self.crushDir)) return;
                    self.StopShaking();
                    self.MoveHNaive(-direction.X);
                    self.MoveVNaive(-direction.Y);
                    InstantAttack(self, -self.crushDir);
                    break;
                case DashCollisionResults.NormalCollision:
                case DashCollisionResults.NormalOverride:
                case DashCollisionResults.Ignore:
                    break;
            }
        }
    }

    private static void ILMoveHCheck(ILContext il)
    {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, static match => match.MatchLdnull())) throw new Exception("Failed to match ldnull for custom Kevin controller.");
        cur.EmitPop();
        cur.EmitLdarg0();
        cur.EmitDelegate(SmuggleKevin);
        cur.EmitDelegate(LoadKevinCollide);
    }

    private static void ILMoveVCheck(ILContext il)
    {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, static match => match.MatchLdnull())) throw new Exception("Failed to match ldnull for custom Kevin controller.");
        cur.EmitPop();
        cur.EmitLdarg0();
        cur.EmitDelegate(SmuggleKevin);
        cur.EmitDelegate(LoadKevinCollide);
    }
    
    
    private static void InstantAttack(CrushBlock self, Vector2 direction)
    {
        if (self.currentMoveLoopSfx == null)
        {
            Audio.Play("event:/game/06_reflection/crushblock_activate", self.Center);
            self.Add(self.currentMoveLoopSfx = new SoundSource());
            self.currentMoveLoopSfx.Position = new Vector2(self.Width, self.Height) / 2f;
            if (SaveData.Instance != null && SaveData.Instance.Name != null && SaveData.Instance.Name.StartsWith("FWAHAHA", StringComparison.InvariantCultureIgnoreCase))
                self.currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop_covert");
            else
                self.currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop");
        }

        self.face.Play("hit");
        self.crushDir = direction;
        self.ClearRemainder();
        self.TurnOffImages();
        self.ActivateParticles(self.crushDir);
        if (self.crushDir.X < 0f)
        {
            foreach (Image activeLeftImage in self.activeLeftImages)
                activeLeftImage.Visible = true;

            self.nextFaceDirection = "left";
        }
        else if (self.crushDir.X > 0f)
        {
            foreach (Image activeRightImage in self.activeRightImages)
                activeRightImage.Visible = true;

            self.nextFaceDirection = "right";
        }
        else if (self.crushDir.Y < 0f)
        {
            foreach (Image activeTopImage in self.activeTopImages)
                activeTopImage.Visible = true;

            self.nextFaceDirection = "up";
        }
        else if (self.crushDir.Y > 0f)
        {
            foreach (Image activeBottomImage in self.activeBottomImages)
                activeBottomImage.Visible = true;

            self.nextFaceDirection = "down";
        }

        bool flag = true;
        if (self.returnStack.Count > 0)
        {
            CrushBlock.MoveState moveState = self.returnStack[^1];
            if (moveState.Direction == direction || moveState.Direction == -direction)
                flag = false;
        }

        if (flag)
            self.returnStack.Add(new CrushBlock.MoveState(self.Position, self.crushDir));
    }

    internal static void HandleHoldableHit(CrushBlock crushBlock, CollisionData data)
    {
        if (crushBlock.Scene.Tracker.GetEntity<CustomKevinController>() is not CustomKevinController ctrl) return;
        if (!ctrl.HitByHoldables) return;
        if (!crushBlock.CanActivate(-data.Direction)) return;
        crushBlock.Attack(-data.Direction);
    }
    
    private static void OnTheoCrystalCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
    {
        orig(self, data);
        if (data.Hit is CrushBlock block) HandleHoldableHit(block, data);
    }

    private static void OnGliderCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
    {
        orig(self, data);
        if (data.Hit is CrushBlock block) HandleHoldableHit(block, data);
    }
    
    
    private static void ILCrushBlockAttackCoro(ILContext il)
    {
        ILCursor cursor = new(il);
        static float ReplaceSpeed(float orig, CrushBlock self) => self.Scene.Tracker.GetEntity<CustomKevinController>() is CustomKevinController ctrl ? ctrl.Speed : orig;
        static float ReplaceAcceleration(float orig, CrushBlock self) => self.Scene.Tracker.GetEntity<CustomKevinController>() is CustomKevinController ctrl ? ctrl.Acceleration : orig;
        static float ReplaceReturnSpeed(float orig, CrushBlock self) => self.Scene.Tracker.GetEntity<CustomKevinController>() is CustomKevinController ctrl ? ctrl.ReturnSpeed : orig;
        static float ReplaceReturnAcceleration(float orig, CrushBlock self) => self.Scene.Tracker.GetEntity<CustomKevinController>() is CustomKevinController ctrl ? ctrl.ReturnAcceleration : orig;
        int index = cursor.Index;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == CrushBlock.CrushSpeed))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(ReplaceSpeed);
        }
        cursor.Index = index;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == CrushBlock.CrushAccel))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(ReplaceAcceleration);
        }
        cursor.Index = index;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == CrushBlock.ReturnSpeed))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(ReplaceReturnSpeed);
        }
        cursor.Index = index;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == CrushBlock.ReturnAccel))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(ReplaceReturnAcceleration);
        }
    }
}
