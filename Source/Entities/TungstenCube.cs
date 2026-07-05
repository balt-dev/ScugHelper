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
using System.Linq;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Anvil")]
public class TungstenCube : Actor, IHasSpeed {
    private Vector2 Speed;
    private readonly Holdable Hold;
    private readonly Image Image;
    private readonly bool KillOnDestroy;
    private readonly bool NoLeaveBehind;
    private readonly bool CrystalSounds;
    private readonly Collider CrushCollider;

    private Level Level { get => SceneAs<Level>(); }
    
    private readonly Hitbox PlayerHitbox = new(8f, 6f, -4f, -2f);
    private readonly Hitbox InvertedCrushHitbox = new(8f, 16f, -4f, -14f);
    private readonly Hitbox CrushHitbox = new(8f, 16f, -4f, -2f);
    private readonly Hitbox PickupHitbox = new(12f, 12f, -6f, -8f);
    
    Vector2 IHasSpeed.Speed { get => Speed; set => Speed = value; }
    public TungstenCube(EntityData data, Vector2 offset) : this(
        data.Position + offset, data.String("Texture", "objects/ScugHelper/anvil"),
        data.Bool("KillOnDestroy"), data.Bool("NoLeaveBehind"), data.Bool("CrystalSounds")
    ) { }

    public TungstenCube(
        Vector2 position, string texturePath = "objects/ScugHelper/anvil",
        bool killOnDestroy = false, bool noLeaveBehind = false, bool crystalSounds = false
    ) : base(position) {
        Depth = 99;
        Collider = PlayerHitbox;
        CrushCollider = CrushHitbox;
        LiftSpeedGraceTime = 1f / 30f;
        CrystalSounds = crystalSounds;
        Image = new Image(GFX.Game[texturePath]) { Position = Center - new Vector2(0, 1) };
        Image.CenterOrigin();
        KillOnDestroy = killOnDestroy;
        NoLeaveBehind = noLeaveBehind;
        Add(Hold = new Holdable() {
            OnHitSpring = HitSpring,
            PickupCollider = PickupHitbox,
            SlowRun = true,
            SpeedGetter = () => Speed,
            SpeedSetter = (value) => Speed = value,
            OnPickup = OnPickup,
            OnRelease = OnRelease
        });
        Add(playerCollider = new PlayerCollider(OnPlayer, CrushCollider));
        Add(new PufferCollider(OnPuffer));
        Add(new TouchSwitchCollider(ts => ts.TurnOn()));
        Add(new SpringCollider(spring => { if (HitSpring(spring)) spring.BounceAnimate(); }));
        Add(new MirrorReflection());
    }

    private void OnPuffer(Puffer puffer) {
        if (!(puffer.state == Puffer.States.Gone || !(puffer.cantExplodeTimer <= 0f))) {
            Speed = (Center - puffer.Center).SafeNormalize(-Vector2.UnitY) * 400f;
            puffer.Explode();
            puffer.GotoGone();
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        
        if (NoLeaveBehind)
            foreach (TungstenCube entity in Level.Tracker.GetEntities<TungstenCube>())
                if (entity != this && entity.Hold.IsHeld && entity.NoLeaveBehind)
                    RemoveSelf();
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
        playerCollider.Collider = GravityHelperImports.IsInverted(this) ? InvertedCrushHitbox : CrushHitbox;
        Image.Position = new(MathF.Floor(Center.X), MathF.Floor(Center.Y) - 1);
        if (Hold.IsHeld) {
            Tag |= Tags.TransitionUpdate;
            Image.Position.Y -= Hold.Holder.IsInverted() ? -4f : 4f;
        } else {
            Tag &= ~Tags.TransitionUpdate;
        }
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
        EnforceLevelBounds();
        if (Top > level.Bounds.Bottom) {
            if (KillOnDestroy) Kill();
            RemoveSelf();
            return;
        }
        MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
    }

    private void EnforceLevelBounds() {
        if (Right >= Level.Bounds.Right)
            Right = Level.Bounds.Right;
        else if (Left < Level.Bounds.Left)
            Left = Level.Bounds.Left;
        else if (Top < (Level.Bounds.Top - 4)) {
            Top = Level.Bounds.Top + 4;
            Speed.Y = 0f;
        }
        else if (Bottom > Level.Bounds.Bottom && KillOnDestroy && SaveData.Instance.Assists.Invincible) {
            Bottom = Level.Bounds.Bottom;
            Speed.Y = -300f;
            Audio.Play("event:/game/general/assist_screenbottom", Position);
        }
        if (X < (Level.Bounds.Left + 10))
            MoveH(32f * Engine.DeltaTime);
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        if (data.Hit is DashBlock block)
            block.Break(Position, Vector2.UnitY * Math.Sign(Speed.Y), true, true);
        if (data.Hit is FastfallBlock fblock)
            fblock.Break(Vector2.UnitX * Math.Sign(Speed.X), true, true);
        if (Speed.Y > 60f)
        {
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Level.DirectionalShake(Vector2.UnitY, 0.1f);
        }
        if (CrystalSounds) {
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Speed.Y);
            Audio.Play("event:/game/03_resort/platform_vert_end", Position);
        } else {
            if (Speed.Y > 60f)
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);
            if (MathF.Abs(Speed.Y) > 20f)
                Audio.Play("event:/game/03_resort/platform_vert_end", Position);
        }
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
        if (CrystalSounds)
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        else
            Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
    }

    public override void OnSquish(CollisionData data) {
        if (!TrySquishWiggle(data, 8, 8)) {
            Audio.Play("event:/game/general/wall_break_stone", Position);
            Level.DirectionalShake(data.Direction, 0.1f);
            SceneAs<Level>().ParticlesFG.Emit(Refill.P_Shatter, 8, Position, Vector2.One * 4f, data.Direction.Angle());
            if (KillOnDestroy) Kill();
            RemoveSelf();
        }
    }

    private void Kill() => Scene.Tracker.GetEntity<Player>()?.Die(Vector2.Zero);

    private static ILHook? getCameraTargetHook;
    [OnLoad]
    public static void LoadHooks() {
        IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
        On.Celeste.Player.Jump += CanJumpHook;
        On.Celeste.Player.SuperJump += CanSuperJumpHook;
        On.Celeste.Player.WallJump += CanWallJumpHook;
        On.Celeste.Player.SuperWallJump += CanSuperWallJumpHook;
        On.Celeste.Player.SuperBounce += BounceHook;
        On.Celeste.Player.SideBounce += SideBounceHook;
        On.Celeste.Level.EnforceBounds += OnLevelEnforceBounds;
        getCameraTargetHook = new(typeof(Player).GetProperty("CameraTarget", BindingFlags.Public | BindingFlags.Instance)!.GetGetMethod()!, GetCameraTargetHook);
    }
    [OnUnload]
    public static void UnloadHooks() {
        IL.Celeste.Player.NormalUpdate -= ModNormalUpdate;
        On.Celeste.Player.Jump -= CanJumpHook;
        On.Celeste.Player.SuperJump -= CanSuperJumpHook;
        On.Celeste.Player.WallJump -= CanWallJumpHook;
        On.Celeste.Player.SuperWallJump -= CanSuperWallJumpHook;
        On.Celeste.Player.SuperBounce -= BounceHook;
        On.Celeste.Player.SideBounce -= SideBounceHook;
        On.Celeste.Level.EnforceBounds -= OnLevelEnforceBounds;
        getCameraTargetHook?.Dispose();
    }

    private static void GetCameraTargetHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNextBestFit(MoveType.After, static instr => instr.MatchCall(typeof(Vector2).GetConstructor([typeof(float), typeof(float)])!)))
            throw new Exception("Tungsten cube failed to match code for camera target offset hook.");
        cur.EmitLdarg0();
        cur.EmitLdloc1();
        static Vector2 Del(Player self, Vector2 vector) {
            if (self.Holding is not { Entity: TungstenCube, IsHeld: true }) return vector;
            if (self.IsInverted()) return vector + Vector2.UnitY * (-30f + Math.Clamp((240f - self.Speed.Y) * 0.24f, -240f, 0f));
            return vector + Vector2.UnitY * Math.Clamp((self.Speed.Y - 240f) * 0.24f, 0f, 240f);
        }
        cur.EmitDelegate(Del);
        cur.EmitStloc1();
    }

    private static bool SideBounceHook(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
        bool res = orig(self, dir, fromX, fromY);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
        return res;
    }

    private static void BounceHook(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
        orig(self, fromY);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static readonly float JumpMultiplier = 0.5f;
    private readonly PlayerCollider playerCollider;

    private static void CanSuperWallJumpHook(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
        orig(self, dir);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static void CanWallJumpHook(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
        orig(self, dir);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) {
            var mult = self.OnGround() ? JumpMultiplier : 0.4f;
            self.Speed.Y *= mult;
            self.varJumpSpeed *= mult;
        }
    }

    private static void CanSuperJumpHook(On.Celeste.Player.orig_SuperJump orig, Player self) {
        orig(self);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    private static void CanJumpHook(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
        orig(self, particles, playSfx);
        if (self.Holding is { Entity: TungstenCube, IsHeld: true }) { self.Speed.Y *= JumpMultiplier; self.varJumpSpeed *= JumpMultiplier; }
    }

    static float FloatMultiply(Player player) {
        if (player.Holding is { Entity: TungstenCube, IsHeld: true })
            return 500f / 160f;
        else
            return 1.0f;
    }

    private static void ModNormalUpdate(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float)instr.Operand == 160f || (float)instr.Operand == 240f))) {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(FloatMultiply);
            cursor.Emit(OpCodes.Mul);
        }
    }

    private static void OnLevelEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player) {
        bool anyNoLeave = false;
        foreach (TungstenCube cube in self.Tracker.GetEntities<TungstenCube>())
            if (anyNoLeave = cube.NoLeaveBehind)
                break;
        if (anyNoLeave && (player.Holding == null || !player.Holding.IsHeld)) {
            if (player.Right > self.Bounds.Right - 1)
                player.Right = self.Bounds.Right - 1;
            if (player.Top < self.Bounds.Top + 1)
                player.Top = self.Bounds.Top + 1;
        }
        orig(self, player);
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
    private static void SpawnCube(bool theo = false) {
        Scene scene = Engine.Instance.scene;
        if (scene is not Level level) return;
        Player? player = level.Tracker.GetEntity<Player>();
        if (player is not Player p) return;
        scene.Add(new TungstenCube(
            p.Position - Vector2.UnitY * 10f,
            theo ? "objects/ScugHelper/theoAnvil" : "objects/ScugHelper/anvil",
            theo, theo
        ));
    }
}
