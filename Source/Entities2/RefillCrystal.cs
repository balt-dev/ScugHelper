#pragma warning disable IDE0130
#nullable enable

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
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/RefillHoldCrystal")]
public class RefillCrystal : Actor, IHasSpeed
{

    private static readonly Vector2 ImageOrigin = new(16, 26);
    private static VirtualRenderTarget? Scratch;

    private readonly Holdable Hold;
    private readonly MTexture Background;
    private readonly MTexture Overlay;

    private readonly MTexture[] particleTextures;
    private readonly DreamBlock.DreamParticle[] particles;
    private float animTimer;
    private Level? Level;
    private Color BackgroundColor;
    private readonly string FallbackRefillType;
    private PlayerCollider[] OnShatter = [];
    private VertexLight? light;
    private BloomPoint? bloom;
    private bool Flash = false;
    private Refill? ClosestRefill;

    public RefillCrystal(EntityData data, Vector2 offset) : base(data.Position + offset) {
        FallbackRefillType = data.String("FallbackRefill", "");
        Depth = 100;
        Collider = new Hitbox(8f, 10f, -4f, -10f);
        Background = GFX.Game["objects/ScugHelper/dreamCrystal/background"];
        Overlay = GFX.Game["objects/ScugHelper/dreamCrystal/overlay"];
        Add(Hold = new Holdable() {
            PickupCollider = new Hitbox(16f, 22f, -8f, -16f),
            OnPickup = OnPickup,
            OnRelease = OnRelease,
            OnHitSpring = OnHitSpring
        });
        Add(new MirrorReflection());
        particleTextures = [
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        ];
        BackgroundColor = data.HexColor("BackgroundColor", Color.Black);
        Color[] particleColors = data.String("ParticleColors", "FFEF11,FF00D0,08a310").Split(',').Select((color) => Calc.HexToColor(color)).ToArray();

        particles = new DreamBlock.DreamParticle[48];
        for (int i = 0; i < particles.Length; i++) {
            particles[i].Position = new Vector2(Calc.Random.Range(0, 32), Calc.Random.Range(0, 32));
            particles[i].Layer = Calc.Random.Choose(0, 1, 1, 1, 2, 2);
            particles[i].TimeOffset = Calc.Random.NextFloat();
            particles[i].Color = Color.LightGray * (0.5f + particles[i].Layer / 2f * 0.5f);
            particles[i].Color = Calc.Random.Choose(particleColors);
        }
        Add(light = new VertexLight(Color.White, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.6f, 48f));
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Level = scene as Level;
        if (Level is null) RemoveSelf();
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        foreach (Entity entity in scene.Entities)
            if (entity is Refill refill && CollideCheck(refill) && (ClosestRefill is null || (ClosestRefill.Center - Center).LengthSquared() < (refill.Center - Center).LengthSquared()))
                ClosestRefill = refill;
        bool needsAdd = ClosestRefill == null;
        ClosestRefill ??= FallbackRefillType switch {
            "OneDash" => new Refill(Position, false, true),
            "TwoDash" => new Refill(Position, true, true),
            "Midair" => new MidairRefill(Position, true),
            "Overcharge" => new OverchargeRefill(Position, true),
            "Limbo" => new LimboRefill(Position, true),
            "DreamTunnel" when CommunalHelperInterop.Loaded => _CreateDreamTunnelRefill(),
            _ => null
        };
        if (ClosestRefill is null) {
            Logger.Warn(nameof(ScugHelper), $"No valid refill for refill crystal! Removing... (Fallback: '{FallbackRefillType}')");
            RemoveSelf();
            return;
        }
        if (needsAdd)
            scene.Add(ClosestRefill);
        ClosestRefill.Tag |= Tags.Persistent;
        ClosestRefill.Visible = false;
        ClosestRefill.Position = Vector2.One * -1e20f;
        OnShatter = ClosestRefill.Components.GetAll<PlayerCollider>().ToArray();
    }

    // WILL HARD CRASH WITHOUT COMMUNALHELPER
    Refill _CreateDreamTunnelRefill() => new CommunalHelper.DashStates.DreamTunnelRefill(new() {Position = Position, Values = new([new("oneUse", true)])}, Vector2.Zero);


    private float noGravityTimer;
    private Vector2 speed;
    public Vector2 Speed { get => speed; set => speed = value; }

    public void OnPickup() {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }

    public void OnRelease(Vector2 force) {
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
            force.Y = -0.4f;

        Speed = force * 200f;
        if (Speed != Vector2.Zero)
            noGravityTimer = 0.1f;
    }

    public override void Update() {
        base.Update();
        TheoUpdate();
        ClosestRefill?.sine.counter = 0;

        animTimer += 6f * Engine.DeltaTime;
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        ClosestRefill?.RemoveSelf();
    }

    float hardVerticalHitSoundCooldown;
    float swatTimer;
    Vector2 prevLiftSpeed;
    Vector2 prevPosition;

    void TheoUpdate() {
        base.Update();
        if (Scene is null) return;
        if (Level is null) return;
        if (Collider is null) return;

        if (Flash) { Collidable = false; return; }

        if (swatTimer > 0f)
            swatTimer -= Engine.DeltaTime;

        hardVerticalHitSoundCooldown -= Engine.DeltaTime;

        if (Hold.IsHeld) {
            Depth = -1;
            prevLiftSpeed = Vector2.Zero;
        } else {
            Depth = 100;
            if (OnGround()) {
                float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                speed.X = Calc.Approach(speed.X, target, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = LiftSpeed;
                if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
                    speed = prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                    speed.Y = Math.Min(speed.Y * 0.6f, 0f);
                    if (speed.X != 0f && speed.Y == 0f)
                        speed.Y = -60f;

                    if (speed.Y < 0f)
                        noGravityTimer = 0.15f;
                } else {
                    prevLiftSpeed = liftSpeed;
                    if (liftSpeed.Y < 0f && speed.Y < 0f)
                        speed.Y = 0f;
                }
            } else if (Hold.ShouldHaveGravity) {
                float num = 800f;
                if (Math.Abs(speed.Y) <= 30f)
                    num *= 0.5f;

                float num2 = 350f;
                if (speed.Y < 0f)
                    num2 *= 0.5f;

                speed.X = Calc.Approach(speed.X, 0f, num2 * Engine.DeltaTime);
                if (noGravityTimer > 0f)
                    noGravityTimer -= Engine.DeltaTime;
                else
                    speed.Y = Calc.Approach(speed.Y, 200f, num * Engine.DeltaTime);
            }

            prevPosition = ExactPosition;
            MoveH(speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(speed.Y * Engine.DeltaTime, OnCollideV);
            if (Center.X > Level.Bounds.Right) {
                Right = Level.Bounds.Right;
                speed.X *= -0.4f;
            } else if (Left < Level.Bounds.Left) {
                Left = Level.Bounds.Left;
                speed.X *= -0.4f;
            } else if (Top < Level.Bounds.Top - 4) {
                Top = Level.Bounds.Top + 4;
                speed.Y = 0f;
            } else if (Top > Level.Bounds.Bottom) RemoveSelf();

            if (X < Level.Bounds.Left + 10)
                MoveH(32f * Engine.DeltaTime);

            Player entity = Scene.Tracker.GetEntity<Player>();
            TempleGate templeGate = CollideFirst<TempleGate>();
            if (templeGate != null && entity != null) {
                templeGate.Collidable = false;
                MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                templeGate.Collidable = true;
            }
        }

        Hold.CheckAgainstColliders();
    }

    public bool OnHitSpring(Spring spring) {
        if (!Hold.IsHeld) {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f) {
                speed.X *= 0.5f;
                speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f) {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = 220f;
                speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f) {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = -220f;
                speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
        }

        return false;
    }

    public void OnCollideH(CollisionData data) {
        if (data.Hit is DashSwitch ds)
            ds.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        if (data.Hit is CrushBlock block)
            CustomKevinController.HandleHoldableHit(block, data);

        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        if (Math.Abs(Speed.X) > 100f)
            ImpactParticles(data.Direction);

        speed.X *= -0.4f;
    }

    public void OnCollideV(CollisionData data) {
        if (data.Hit is DashSwitch ds)
            ds.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        if (data.Hit is CrushBlock block)
            CustomKevinController.HandleHoldableHit(block, data);

        if (Speed.Y > 0f) {
            if (hardVerticalHitSoundCooldown <= 0f) {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                hardVerticalHitSoundCooldown = 0.5f;
            } else
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
        }

        if (Speed.Y > 160f)
            ImpactParticles(data.Direction);

        if (Speed.Y > 140f && !(data.Hit is SwapBlock or DashSwitch))
            speed.Y *= -0.6f;
        else
            speed.Y = 0f;
    }

    public void ImpactParticles(Vector2 dir) {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        if (dir.X > 0f) {
            direction = MathF.PI;
            position = new Vector2(Right, Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        } else if (dir.X < 0f) {
            direction = 0f;
            position = new Vector2(Left, Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        } else if (dir.Y > 0f) {
            direction = -MathF.PI / 2f;
            position = new Vector2(X, Bottom);
            positionRange = Vector2.UnitX * 6f;
        } else {
            direction = MathF.PI / 2f;
            position = new Vector2(X, Top);
            positionRange = Vector2.UnitX * 6f;
        }

        Level?.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
    }

    public override void Render() {
        base.Render();
        if (ClosestRefill is null) return;
        if (Flash) {
            Background.Draw(Position, ImageOrigin, Color.White);
            return;
        }
        Scratch ??= VirtualContent.CreateRenderTarget($"DreamCrystalScratch", 32, 32);
        var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();
        GameplayRenderer.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Scratch);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Engine.Graphics.GraphicsDevice.Clear(BackgroundColor);
        RenderStars();
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Utils.AlphaMaskBlendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Background.Draw(Vector2.Zero);
        Draw.SpriteBatch.End();

        var gravScale = GravityHelperImports.IsActorInverted?.Invoke(this) ?? false ? new(1, -1) : Vector2.One;
        Vector2 snappedPosition = new(MathF.Round(Position.X), MathF.Round(Position.Y)); // MotionSmoothing

        Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        GameplayRenderer.Begin();
        Background.Draw(snappedPosition, ImageOrigin, Color.Black, gravScale);
        GameplayRenderer.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Utils.AdditiveKeepAlphaBlendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, GameplayRenderer.instance.Camera.Matrix);
        Draw.SpriteBatch.Draw(Scratch, snappedPosition + (gravScale.Y > 0 ? Vector2.Zero : Vector2.UnitY * 20), null, Color.White, 0f, ImageOrigin, 1f, gravScale.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();

        GameplayRenderer.Begin();
        Overlay.Draw(snappedPosition, ImageOrigin, Color.White, gravScale);
    }

    private void RenderStars() {
        if (Level is null) return;
        if (Level.Camera is null) return;
        Vector2 cameraPos = Level.Camera.Position;
        if (Level.Session.GetFlag("ScugHelper.GladelineCrystal")) {
            Vector2 gladPos = Vector2.Zero;
            gladPos += (cameraPos - Position) * (0.3f + 0.25f);
            PutInside(ref gladPos);
            gladPos -= Vector2.One * 16f;
            GFX.Portraits["madeline/normal00"].Draw(gladPos, Vector2.Zero, Color.White, Vector2.One * 1f / 5f);
        } else {
            for (int i = 0; i < particles.Length; i++) {

                int layer = particles[i].Layer;
                Vector2 particlePos = particles[i].Position;
                particlePos += (cameraPos - Position) * (0.3f + 0.25f * layer);
                PutInside(ref particlePos);
                Color color = particles[i].Color;
                MTexture mTexture = layer switch {
                    0 => particleTextures[3 - (int)((particles[i].TimeOffset * 4f + animTimer) % 4f)],
                    1 => particleTextures[1 + (int)((particles[i].TimeOffset * 2f + animTimer) % 2f)],
                    2 => particleTextures[2],
                    _ => throw new UnreachableException()
                };

                mTexture.DrawCentered(particlePos, color);
            }
        }
    }

    private void PutInside(ref Vector2 pos) {
        pos.X = ((pos.X % 32) + 32) % 32;
        pos.Y = ((pos.Y % 32) + 32) % 32;
    }

    [OnLoad]
    internal static void LoadHooks() => On.Celeste.Player.NormalUpdate += OnPlayerNormalUpdate;
    [OnUnload]
    internal static void UnloadHooks() => On.Celeste.Player.NormalUpdate -= OnPlayerNormalUpdate;

    private static int OnPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
        var res = orig(self);
        if (self.Holding?.Entity is RefillCrystal holdCrys && (Input.Dash.Pressed || Input.CrouchDash.Pressed) && self.Dashes > 0) {
            self.Dashes = Math.Max(0, self.Dashes - 1);
            self.Speed += self.LiftBoost;
            res = self.StartDash();
            self.Holding = null;
            holdCrys.Collidable = false;
            holdCrys.Hold.Holder = null;

            // fuck it we ball
            for (int i = 0; i < 12; i++)
                Audio.Play("event:/game/06_reflection/fall_spike_smash");

            holdCrys.Add(new Coroutine(holdCrys.FlashRemove()));
            Vector2? oldPos = holdCrys.ClosestRefill?.Position;
            holdCrys.ClosestRefill?.Position = holdCrys.Position;
            foreach (var onShatter in holdCrys.OnShatter)
                onShatter.OnCollide(self);
            holdCrys.ClosestRefill?.Position = oldPos ?? Vector2.Zero;
        }
        return res;
    }

    private IEnumerator FlashRemove() {
        Flash = true;
        yield return null;
        Celeste.Freeze(0.05f);
        yield return 0.05f;
        CrystalDebris.Burst(Position, Color.White, false, 16);
        Level?.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f);
        RemoveSelf();
    }
}
