#nullable enable

using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Anvil")]
public class TungstenCube : Actor, IHasSpeed
{
    private Vector2 Speed;
    private readonly Holdable Hold;
    private readonly Image Image;
    private Collider CrushCollider;

    private Level Level { get => SceneAs<Level>(); }
    Vector2 IHasSpeed.Speed { get => Speed; set => Speed = value; }
    public TungstenCube(EntityData data, Vector2 offset) : this(data.Position + offset) { }

    public TungstenCube(Vector2 position) : base(position) {
        Depth = -20;
        Collider = new Hitbox(8f, 6f, -4f, -2f);
        CrushCollider = new Hitbox(8f, 16f, -4f, -2f);
        LiftSpeedGraceTime = 1f / 30f;
        Image = new Image(GFX.Game["objects/ScugHelper/anvil"]);
        Image.Position = TopLeft;
        Image.Position.Y -= 2f;
        Add(Hold = new Holdable() {
            OnHitSpring = HitSpring,
            PickupCollider = new Hitbox(12f, 12f, -6f, -8f),
            SlowRun = true,
            SpeedGetter = () => Speed,
            SpeedSetter = (value) => Speed = value,
            OnPickup = OnPickup,
            OnRelease = OnRelease
        });
        Add(playerCollider = new PlayerCollider(OnPlayer, CrushCollider));
    }
    public void OnPickup() {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }

    private void OnRelease(Vector2 force) {
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
            force.Y = -0.8f;

        Speed = force * 250f;
    }

    public override void Render() {
        base.Render();
        Image.DrawSimpleOutline();
        Image.Render();
    }

    private void OnPlayer(Player player) {
        if (Speed.Y - player.AdjustedSpeed().Y >= 300 && !Hold.IsHeld) {
            if (player.wasOnGround)
                player.Die(Vector2.Zero);
            else {
                Audio.Play("event:/game/general/thing_booped");
                player.SetAdjustedSpeed(player.AdjustedSpeed().X, Speed.Y);
                Celeste.Freeze(0.1f);
            }
        }

    }

    public bool HitSpring(Spring spring) {
        switch (spring.Orientation) {
            default:
                if (Speed.Y >= 0f) {
                    Speed = 250f * -Vector2.UnitY;
                    MoveTowardsX(spring.CenterX, 4f);
                    return true;
                }
                return false;
            case Spring.Orientations.WallLeft:
                if (Speed.X <= 60f) {
                    Speed = 400f * Vector2.UnitX;
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
            case Spring.Orientations.WallRight:
                if (Speed.X >= -60f) {
                    Speed = 400f * Vector2.UnitX;
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
        }
    }

    private static readonly float Gravity = 1500f;
    private static readonly float TerminalVelocity = 1000f;
    private Vector2 prevLiftSpeed;

    private bool WasOnGround = false;

    public override void Update() {
        base.Update();
        if (GravityHelperImports.IsInverted(this))
            playerCollider.Collider = new Hitbox(8f, 16f, -4f, -14f);
        else
            playerCollider.Collider = new Hitbox(8f, 16f, -4f, -2f);
        Image.Position = new(MathF.Floor(Left), MathF.Floor(Top) - 2);
        if (Hold.IsHeld)
            Image.Position.Y -= Hold.Holder.IsInverted() ? -4f : 4f;
        Image.Update();
        if (!Hold.IsHeld) {
            if (CollideFirst<HeartGem>() is HeartGem gem && gem != null) {
                Player player = Level.Tracker.GetEntity<Player>();
                if (player != null && !gem.collected) {
                    gem.Collect(player);
                }
            }

        }
        if (LiftSpeed.Length() < prevLiftSpeed.Length())
            Speed += prevLiftSpeed;
        else if (OnGround() && WasOnGround && Speed.Y > 0f)
            Speed.Y = 0;
        WasOnGround = OnGround();
        prevLiftSpeed = LiftSpeed;
        foreach (AnvilCollider component in Scene.Tracker.GetComponents<AnvilCollider>())
            component.Check(this);
        Speed.X = Calc.Approach(Speed.X, 0, 700f * Engine.DeltaTime);
        Speed.Y = Calc.Approach(Speed.Y, 0, 400f * Engine.DeltaTime);
        if (!OnGround())
            Speed.Y += Gravity * Engine.DeltaTime;
        Speed.Y = Math.Min(Speed.Y, TerminalVelocity);
        if (Hold.IsHeld)
            Speed = Vector2.Zero;
        var level = SceneAs<Level>();
        var movedPos = Position + (Speed * Engine.DeltaTime);
        if (level.Bounds.Left > movedPos.X || movedPos.X > level.Bounds.Right) Speed.X *= -1;
        if (Position.Y > level.Bounds.Bottom) { RemoveSelf(); return; }
        MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
    }

    private void OnCollideV(CollisionData data) {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        if (data.Hit is DashBlock block)
            block.Break(Position, Vector2.UnitY * Math.Sign(Speed.Y), true, true);
        if (data.Hit is FastfallBlock fblock)
            fblock.Break(Vector2.UnitX * Math.Sign(Speed.X), true, true);
        if (Speed.Y > 60f) {
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Level.DirectionalShake(Vector2.UnitY, 0.1f);
        }
        if (Speed.Y > 60f)
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);
        if (MathF.Abs(Speed.Y) > 20f)
            Audio.Play("event:/game/03_resort/platform_vert_end", Position);
        Speed.Y *= -0.3f;
    }

    private void OnCollideH(CollisionData data) {
        if (data.Hit is CrushBlock crushBlock)
            CustomKevinController.HandleHoldableHit(crushBlock, data);
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        if (data.Hit is DashBlock block)
            block.Break(Position, Vector2.UnitX * Math.Sign(Speed.X), true, true);
        if (data.Hit is FastfallBlock fblock)
            fblock.Break(Vector2.UnitX * Math.Sign(Speed.X), true, true);
        Speed.X *= -0.8f;
        Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
    }

    public override void OnSquish(CollisionData data) {
        if (!TrySquishWiggle(data, 8, 8)) {
            Audio.Play("event:/game/general/wall_break_stone", Position);
            Level.DirectionalShake(data.Direction, 0.1f);
            SceneAs<Level>().ParticlesFG.Emit(Refill.P_Shatter, 8, Position, Vector2.One * 4f, data.Direction.Angle());
            RemoveSelf();
        }
    }
    private static ILHook? getCameraTargetHook;
    [OnLoad]
    public static void LoadHooks() {
        IL.Celeste.Player.NormalBegin += ModNormalBegin;
        IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
        On.Celeste.TouchSwitch.ctor_Vector2 += TouchSwitchCtorHook;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += SpringCtorHook;
        On.Celeste.Puffer.ctor_Vector2_bool += PufferCtorHook;
        On.Celeste.Player.Jump += CanJumpHook;
        On.Celeste.Player.SuperJump += CanSuperJumpHook;
        On.Celeste.Player.WallJump += CanWallJumpHook;
        On.Celeste.Player.SuperWallJump += CanSuperWallJumpHook;
        On.Celeste.Player.SuperBounce += BounceHook;
        On.Celeste.Player.SideBounce += SideBounceHook;
        getCameraTargetHook = new(typeof(Player).GetProperty("CameraTarget", BindingFlags.Public | BindingFlags.Instance)!.GetGetMethod()!, GetCameraTargetHook);
    }
    [OnUnload]
    public static void UnloadHooks() {
        IL.Celeste.Player.NormalBegin -= ModNormalBegin;
        IL.Celeste.Player.NormalUpdate -= ModNormalUpdate;
        On.Celeste.TouchSwitch.ctor_Vector2 -= TouchSwitchCtorHook;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= SpringCtorHook;
        On.Celeste.Puffer.ctor_Vector2_bool -= PufferCtorHook;
        On.Celeste.Player.Jump -= CanJumpHook;
        On.Celeste.Player.SuperJump -= CanSuperJumpHook;
        On.Celeste.Player.WallJump -= CanWallJumpHook;
        On.Celeste.Player.SuperWallJump -= CanSuperWallJumpHook;
        On.Celeste.Player.SuperBounce -= BounceHook;
        On.Celeste.Player.SideBounce -= SideBounceHook;
        getCameraTargetHook?.Dispose();
    }

    private static void GetCameraTargetHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNextBestFit(MoveType.After, static instr => instr.MatchCall(typeof(Vector2).GetConstructor([typeof(float), typeof(float)])!)))
            throw new Exception("Tungsten cube failed to match code for camera target offset hook.");
        cur.EmitLdarg0();
        cur.EmitLdloc1();
        static Vector2 Del(Player self, Vector2 vector) {
            if (self.Holding?.Entity is not TungstenCube) return vector;
            if (self.IsInverted()) return vector + Vector2.UnitY * (-30f + Math.Clamp((240f - self.Speed.Y) * 0.24f, -240f, 0f));
            return vector + Vector2.UnitY * Math.Clamp((self.Speed.Y - 240f) * 0.24f, 0f, 240f);
        }
        cur.EmitDelegate(Del);
        cur.EmitStloc1();
    }

    private static bool SideBounceHook(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
        bool res = orig(self, dir, fromX, fromY);
        if (self.Holding?.Entity is TungstenCube) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
        return res;
    }

    private static void BounceHook(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
        orig(self, fromY);
        if (self.Holding?.Entity is TungstenCube) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static void SpringCtorHook(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse) {
        orig(self, position, orientation, playerCanUse);
        self.Add(new AnvilCollider(bumper => { if (bumper.HitSpring(self)) self.BounceAnimate(); }));
    }

    private static void TouchSwitchCtorHook(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position) {
        orig(self, position);
        self.Add(new AnvilCollider(crys => self.TurnOn()));
    }

    private static void PufferCtorHook(On.Celeste.Puffer.orig_ctor_Vector2_bool orig, Puffer self, Vector2 position, bool faceRight) {
        orig(self, position, faceRight);
        self.Add(new AnvilCollider(crys => {
            if (!(self.state == Puffer.States.Gone || !(self.cantExplodeTimer <= 0f))) {
                crys.Speed = (crys.Center - self.Center).SafeNormalize(-Vector2.UnitY) * 400f;
                self.Explode();
                self.GotoGone();
            }
        }));
    }

    private static readonly float JumpMultiplier = 0.5f;
    private readonly PlayerCollider playerCollider;

    private static void CanSuperWallJumpHook(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
        orig(self, dir);
        if (self.Holding?.Entity is TungstenCube) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static void CanWallJumpHook(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
        orig(self, dir);
        if (self.Holding?.Entity is TungstenCube) {
            var mult = self.OnGround() ? JumpMultiplier : 0.4f;
            self.Speed.Y *= mult;
            self.varJumpSpeed *= mult;
        }
    }

    private static void CanSuperJumpHook(On.Celeste.Player.orig_SuperJump orig, Player self) {
        orig(self);
        if (self.Holding?.Entity is TungstenCube) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static void CanJumpHook(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
        orig(self, particles, playSfx);
        if (self.Holding?.Entity is TungstenCube) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    static float FloatMultiply(Player player) {
        if (player.Holding?.Entity is TungstenCube)
            return 500f / 160f;
        else
            return 1.0f;
    }

    private static void ModNormalBegin(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 160f)) {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(FloatMultiply);
            cursor.Emit(OpCodes.Mul);
        }
    }
    private static void ModNormalUpdate(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float)instr.Operand == 160f || (float)instr.Operand == 240f))) {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(FloatMultiply);
            cursor.Emit(OpCodes.Mul);
        }
    }


    [Command("givetheo", "Spawns a theo crystal on the player.")]
    private static void SpawnTheo() {
        Scene scene = Engine.Instance.scene;
        if (scene is not Level level) return;
        Player? player = level.Tracker.GetEntity<Player>();
        if (player is not Player p) return;
        scene.Add(new TheoCrystal(p.Position - Vector2.UnitY * 10f));
    }
    [Command("giveglider", "Spawns a jellyfish on the player.")]
    private static void SpawnGlider() {
        Scene scene = Engine.Instance.scene;
        if (scene is not Level level) return;
        Player? player = level.Tracker.GetEntity<Player>();
        if (player is not Player p) return;
        scene.Add(new Glider(p.Position - Vector2.UnitY * 10f, true, false));
    }

    [Command("givecube", "Spawns a cube on the player.")]
    private static void SpawnCube() {
        Scene scene = Engine.Instance.scene;
        if (scene is not Level level) return;
        Player? player = level.Tracker.GetEntity<Player>();
        if (player is not Player p) return;
        scene.Add(new TungstenCube(p.Position - Vector2.UnitY * 10f));
    }
}

[Tracked(false)]
internal class AnvilCollider(Action<TungstenCube> onCollide) : Component(active: false, visible: false)
{
    public Action<TungstenCube> OnCollide = onCollide;

    public void Check(TungstenCube obj) {
        if (obj.CollideCheck(Entity))
            OnCollide?.Invoke(obj);
    }
}
