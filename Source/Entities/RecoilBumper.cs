using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod.Helpers;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

public class GridCircle : Circle {
    public GridCircle(float radius, float x = 0, float y = 0) : base(radius, x, y) {}
}

[CustomEntity("ScugHelper/RecoilBumper")]
public class RecoilBumper : Actor, IHasSpeed
{
    Vector2 IHasSpeed.Speed { get => Speed; set => Speed = value; }

    private Sprite sprite;
    private VertexLight light;
    private BloomPoint bloom;
    private float respawnTimer;
    public Vector2 Speed;
    public float Mass;
    public float Drag;
    private Collision onCollideH;
    private Collision onCollideV;

    public RecoilBumper(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        LiftSpeedGraceTime = 1f / 30f;
        Mass = data.Float("Mass", 2f);
        Drag = data.Float("Drag", 200f);
        Depth = -20;
        Collider = new GridCircle(12f);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("recoilBumper"));
        Add(light = new VertexLight(Color.Purple, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.5f, 16f));
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        Add(new RecoilBumperCollider(OnOtherBumperCollide));
    }

    private void OnOtherBumperCollide(RecoilBumper bumper) {
        if (bumper == this) return;
        if (Center == bumper.Center) return;
        Vector2 delta_p = Center - bumper.Center;
        Vector2 delta_v = Speed - bumper.Speed;

        // Move them out of each other
        float overlap = 12f * 2 - delta_p.Length();
        Vector2 norm = delta_p.SafeNormalize(Vector2.UnitY);
        Vector2 movement = norm * overlap / 2;
        MoveH(movement.X); MoveV(movement.Y);
        bumper.MoveH(-movement.X); bumper.MoveV(-movement.Y);

        // Set the speeds
        float m1 = Mass;
        float m2 = bumper.Mass;
        float mass_factor = 1f / (m1 + m2);
        float dot_over_len2 = Vector2.Dot(delta_v, delta_p) / delta_p.LengthSquared();
        Speed -= 2 * m2 * mass_factor * dot_over_len2 * delta_p;
        bumper.Speed += 2 * m1 * mass_factor * dot_over_len2 * delta_p;
    }

    public bool HitSpring(Spring spring)
    {
        switch (spring.Orientation)
        {
            default:
                if (Speed.Y >= 0f)
                {
                    Speed.Y = -MathF.Max(MathF.Abs(Speed.Y), 224f);
                    MoveTowardsX(spring.CenterX, 4f);
                    return true;
                }
                return false;
            case Spring.Orientations.WallLeft:
                if (Speed.X <= 60f)
                {
                    Speed.X = -MathF.Max(MathF.Abs(Speed.X), 224f);
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
            case Spring.Orientations.WallRight:
                if (Speed.X >= -60f)
                {
                    Speed.X = MathF.Max(MathF.Abs(Speed.X), 224f);
                    MoveTowardsY(spring.CenterY, 4f);
                    return true;
                }

                return false;
        }
    }

    private Vector2 prevLiftSpeed;

    public override void Update()
    {
        base.Update();
        if (LiftSpeed.Length() < prevLiftSpeed.Length())
            Speed += prevLiftSpeed;
        prevLiftSpeed = LiftSpeed;

        foreach (RecoilBumperCollider component in Scene.Tracker.GetComponents<RecoilBumperCollider>())
            component.Check(this);

        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                light.Visible = true;
                bloom.Visible = true;
                sprite.Play("on");
                Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
            }
        }
        else if (Scene.OnInterval(0.05f))
        {
            float dir = Calc.Random.NextAngle();
            SceneAs<Level>().Particles.Emit(Bumper.P_Ambience, 1, Center + Calc.AngleToVector(dir, 8), Vector2.One * 2f, dir);
        }
        var level = SceneAs<Level>();
        var movedPos = Position + (Speed * Engine.DeltaTime);
        if (level.Bounds.Left > movedPos.X || movedPos.X > level.Bounds.Right)
            Speed.X *= -1;
        if (level.Bounds.Top > movedPos.Y || movedPos.Y > level.Bounds.Bottom)
            Speed.Y *= -1;
        MoveH(Speed.X * Engine.DeltaTime, onCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
        Speed = Calc.Approach(Speed, Vector2.Zero, Drag * Engine.DeltaTime);
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        Speed.Y *= -1;
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch button)
            button.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        Speed.X *= -1;
    }

    public void OnPlayer(Player player)
    {
        if (respawnTimer <= 0f)
        {
            Audio.Play("event:/game/09_core/pinballbumper_hit", Position);

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);

            Vector2 collisionNormal = (Center - player.Center).SafeNormalize(Vector2.UnitY);

            Vector2 oldSpeed = player.Speed;
            player.PointBounce(Center);
            player.Speed += Speed;
            Vector2 speedChange = player.Speed - oldSpeed;
            Speed -= speedChange / Mass;

            SlashFx.Burst(Center, collisionNormal.Angle());


            if (player.LastBooster == null || player.LastBooster is not PinballBooster)
                player.StateMachine.State = Player.StLaunch;

            respawnTimer = 0.6f;
            sprite.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(collisionNormal, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, Center + collisionNormal * 12f, Vector2.One * 3f, collisionNormal.Angle());
        }
    }


    public override void OnSquish(CollisionData data)
    {
        if (!TrySquishWiggle(data, 3, 3))
        {
            data.Hit.Add(new Coroutine(CrushSoundRoutine(Position)));
            SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, 100, Position, Vector2.One * 16f, data.Direction.Angle());
            RemoveSelf();
        }
    }

    private static IEnumerator CrushSoundRoutine(Vector2 pos) {
        var evInstance = Audio.Play("event:/game/09_core/hotpinball_activate", pos);
		yield return 0.5f;
        evInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }


    public static void LoadHooks() {
        if (!HookUtils.TryDisableInlining(typeof(Monocle.Grid).GetMethod("Collide", [typeof(Circle)])))
            throw new Exception("Failed to disable inlining on Monocle.Collide(Circle).");
        On.Monocle.Grid.Collide_Circle += CircleCollide;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += SpringCtorHook;
        On.Celeste.TouchSwitch.ctor_Vector2 += TouchSwitchCtorHook;
    }

    public static void UnloadHooks() {
        On.Monocle.Grid.Collide_Circle -= CircleCollide;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= SpringCtorHook;
        On.Celeste.TouchSwitch.ctor_Vector2 -= TouchSwitchCtorHook;
    }

    private static void TouchSwitchCtorHook(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position)
    {
        orig(self, position);
        self.Add(new RecoilBumperCollider(bumper => self.TurnOn()));
    }

    private static void SpringCtorHook(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        self.Add(new RecoilBumperCollider(bumper => { if (bumper.HitSpring(self)) self.BounceAnimate(); }));
    }

    private static bool CircleCollide(On.Monocle.Grid.orig_Collide_Circle orig, Grid self, Circle circle)
    {
        if (circle is GridCircle)
            return self.Collide(circle.Bounds);
        else return orig(self, circle);
    }
}

[Tracked(false)]
internal class RecoilBumperCollider(Action<RecoilBumper> onCollide): Component(active: false, visible: false) {
    public Action<RecoilBumper> OnCollide = onCollide;

    public void Check(RecoilBumper obj) {
        if (obj.CollideCheck(Entity))
            OnCollide?.Invoke(obj);
    }
}
