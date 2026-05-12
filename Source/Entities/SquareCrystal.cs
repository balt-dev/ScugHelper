using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SquareGem")]
public class SquareCrystal : Actor, IHasSpeed
{
    Vector2 IHasSpeed.Speed { get => Speed; set => Speed = value; }

    public int GemID;
    public EntityID GID;
    public Color InfillColor { get; protected set; }

    private Sprite fillSprite;
    private Sprite glareSprite;
    private Wiggler scaleWiggler;
    private Wiggler moveWiggler;
    private float bounceSfxDelay;
    private Vector2 moveWiggleDir;
    private float moveWiggleStart;
    private static readonly float FRAC_SQRT_2_2 = (float)(Math.Sqrt(2.0) / 2);
    private static readonly float wiggleSettleSpeed = 0.6f;
    private Vector2 Speed;
    private readonly bool DoGravity;

    public SquareCrystal(EntityData data, Vector2 offset, EntityID gid)
    : base(data.Position + offset)
    {
        LiftSpeedGraceTime = 1f / 30f;
        Depth = -15;
        moveWiggleStart = 0.0f;
        InfillColor = data.HexColor("Color", Color.White);
        Collider = new Hitbox(18f, 18f, -9f, -9f);
        DoGravity = data.Bool("Gravity", false);

        Add(scaleWiggler = Wiggler.Create(0.5f, 4f, f => {
            fillSprite.Scale = Vector2.One * (1f + f * 0.3f);
            glareSprite.Scale = Vector2.One * (1f + f * 0.3f);
        }));
        moveWiggler = Wiggler.Create(0.8f, 2f);
        moveWiggler.StartZero = true;
        Add(moveWiggler);
        Add(new PlayerCollider(OnPlayer));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        fillSprite = GFX.SpriteBank.Create("squareGemFill");
        fillSprite.Color = InfillColor;
        Add(fillSprite);
        glareSprite = GFX.SpriteBank.Create("squareGemOutline");
        glareSprite.Color = Color.White;
        Add(glareSprite);
        fillSprite.Play(DoGravity ? "glint" : "spin");
        glareSprite.Play(DoGravity ? "glint" : "spin");
        fillSprite.CenterOrigin();
        glareSprite.CenterOrigin();
        Add(new BloomPoint(0.35f, 24f));
        Add(new VertexLight(InfillColor, 0.3f, 32, 64));
    }

    public void OnPlayer(Player player)
    {
        Vector2 deltaDir = (player.Center - Center).SafeNormalize(Vector2.UnitY);

        Vector2 oldSpeed = player.AdjustedSpeed();
        if (Math.Abs(deltaDir.X) > FRAC_SQRT_2_2)
            player.PointBounce(new(Center.X, player.Y));
        else
        {
            player.PointBounce(new(player.X, Center.Y));
            player.Speed.X = 0;
        }
        player.SetAdjustedSpeed(player.AdjustedSpeed() + Speed);
        Vector2 speedDelta = player.AdjustedSpeed() - oldSpeed;
        if (DoGravity) Speed -= speedDelta;

        moveWiggler.Start();
        scaleWiggler.Start();
        fillSprite.Play(DoGravity ? "glint" : "spin", restart: true);
        glareSprite.Play(DoGravity ? "glint" : "spin", restart: true);
        moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
        moveWiggleStart = Scene.TimeActive;
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        if (bounceSfxDelay <= 0f)
        {
            Audio.Play("event:/game/general/crystalheart_bounce", Position);
            bounceSfxDelay = 0.1f;
        }
    }

    private static readonly float Gravity = 900f;
    private Vector2 prevLiftSpeed;

    private bool WasOnGround = false;

    public override void Update()
    {
        base.Update();
        if (LiftSpeed.Length() < prevLiftSpeed.Length())
            Speed += prevLiftSpeed;
        else if (OnGround() && DoGravity && WasOnGround && Speed.Y > 0f)
            Speed.Y = 0;
        WasOnGround = OnGround();
        prevLiftSpeed = LiftSpeed;
        foreach (SquareCrystalCollider component in Scene.Tracker.GetComponents<SquareCrystalCollider>())
            component.Check(this);
        Speed = Calc.Approach(Speed, Vector2.Zero, 400f * Engine.DeltaTime);
        if (!OnGround() && DoGravity)
            Speed.Y += Gravity * Engine.DeltaTime;
        var level = SceneAs<Level>();
        var movedPos = Position + (Speed * Engine.DeltaTime);
        if (level.Bounds.Left > movedPos.X || movedPos.X > level.Bounds.Right) Speed.X *= -1;
        if (Position.Y > level.Bounds.Bottom) { RemoveSelf(); return; }
        MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
        bounceSfxDelay -= Engine.DeltaTime;
        float wiggleFac = wiggleSettleSpeed / (1f + (Scene.TimeActive - moveWiggleStart));
        fillSprite.Position = wiggleFac * moveWiggleDir * moveWiggler.Value * -8f;
        glareSprite.Position = wiggleFac * moveWiggleDir * moveWiggler.Value * -8f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        Speed.Y *= -0.8f;
        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        Speed.X *= -0.8f;
        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
    }

    public override void OnSquish(CollisionData data) {
        if (!TrySquishWiggle(data, 3, 3))
        {
            for (int i = 0; i < 5; i++)
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
            SceneAs<Level>().ParticlesFG.Emit(Refill.P_Shatter, 30, Position, Vector2.One * 8f, data.Direction.Angle());
            RemoveSelf();
        }
    }
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += SpringCtorHook;
        On.Celeste.TouchSwitch.ctor_Vector2 += TouchSwitchCtorHook;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= SpringCtorHook;
        On.Celeste.TouchSwitch.ctor_Vector2 -= TouchSwitchCtorHook;
    }

    private static void TouchSwitchCtorHook(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position)
    {
        orig(self, position);
        self.Add(new SquareCrystalCollider(crys => self.TurnOn()));
    }

    private static void SpringCtorHook(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        self.Add(new SquareCrystalCollider(crys => { if (crys.HitSpring(self)) self.BounceAnimate(); }));
    }

    public bool HitSpring(Spring spring)
    {
        switch (spring.Orientation)
        {
            default:
                if (Speed.Y >= 0f)
                {
                    Speed = 400f * -Vector2.UnitY;
                    MoveTowardsX(spring.CenterX, 4f);
                    return true;
                }
                return false;
            case Spring.Orientations.WallLeft:
                if (Speed.X <= 60f)
                {
                    Speed = 400f * Vector2.UnitX;
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
            case Spring.Orientations.WallRight:
                if (Speed.X >= -60f)
                {
                    Speed = 400f * Vector2.UnitX;
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
        }
    }
}

[Tracked(false)]
internal class SquareCrystalCollider(Action<SquareCrystal> onCollide): Component(active: false, visible: false) {
    public Action<SquareCrystal> OnCollide = onCollide;

    public void Check(SquareCrystal obj) {
        if (obj.CollideCheck(Entity))
            OnCollide?.Invoke(obj);
    }
}
