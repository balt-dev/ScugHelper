
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Hangrail")]
public class HangRail : Entity
{
    internal bool DoGravity = false;
    internal Holdable Hold;
    internal Sprite Sprite;
    internal Vector2 InitialStart;
    internal Vector2 InitialEnd;
    internal Vector2 Start;
    internal Vector2 End;
    internal Vector2 Speed;
    internal Vector2 RetentionSpeed;
    internal float RetentionTimer;
    internal bool TakeStamina = true;
    internal float NoGrabTimer = 0f;
    static readonly float PlayerOffset = 20f;
    static readonly float RetentionTime = 0.1f;
    static readonly float GrabCooldown = 0.6f;

    internal float PlayerMaxSpeed = 30f;
    internal float Friction = 80f;
    internal float MaxFall = 160f;
    internal float Gravity = 400f;
    internal float HoldSpeedLimit = 60f;
    internal float TiltSpriteThreshold = 20f;
    internal float StaminaCost = 12f;
    internal float JumpStaminaCost = 27.5f;

    private readonly MTexture[] ropeSlices;
    private readonly MTexture tieTexture;
    private readonly Vector2 Direction;
    private bool noLiftBoost = true;

    public HangRail(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        DoGravity = data.Bool("StartWithGravity");
        TakeStamina = data.Bool("TakesStamina", true);

        PlayerMaxSpeed = data.Float("PlayerMaxSpeed", 30f);
        Friction = data.Float("Friction", 80f);
        MaxFall = data.Float("MaxFall", 160f);
        Gravity = data.Float("Gravity", 400f);
        HoldSpeedLimit = data.Float("HoldSpeedLimit", 60f);
        TiltSpriteThreshold = data.Float("TiltSpriteThreshold", 20f);
        StaminaCost = data.Float("StaminaCost", 12f);
        JumpStaminaCost = data.Float("JumpStaminaCost", 27.5f);

        Depth = 10;
        var offsetNodes = data.NodesOffset(offset);
        Start = offsetNodes[0];
        End = offsetNodes[1];
        InitialStart = Start;
        InitialEnd = End;
        Position = Start + (End - Start) * data.Float("Position", 0.0f);
        Direction = (End - Start).SafeNormalize();
        Collider = Start == End ? new Hitbox(16, 16, -8, 8) : new Hitbox(12, 12, -6, 8);
        Add(Sprite = GFX.SpriteBank.Create(data.String("Sprite", "hangrail")));
        Sprite.Play("idle");
        Sprite.CenterOrigin();
        Add(Hold = new()
        {
            SpeedGetter = () => Speed,
            SpeedSetter = (value) => Speed = value,
            OnPickup = OnPickup,
            OnCarry = OnCarry,
            OnRelease = OnRelease,
            OnSwat = OnSwat,
            PickupCollider = Collider
        });

        var tie = data.String("TieSprite", "objects/ScugHelper/hangrail/tie");
        if (tie == "objects/hangrail/tie") tie = "objects/ScugHelper/hangrail/tie";
        tieTexture = GFX.Game[tie];
        var rope = data.String("RopeSprite", "objects/ScugHelper/hangrail/rope");
        if (rope == "objects/hangrail/rope") rope = "objects/ScugHelper/hangrail/rope";
        var ropeTexture = GFX.Game[rope];
        ropeSlices = Enumerable.Range(0, ropeTexture.Width)
            .Select(i => new MTexture(ropeTexture, i, 0, 1, ropeTexture.Height))
            .ToArray();
    }

    private void OnSwat(HoldableCollider collider, int arg2) => OnRelease(Vector2.Zero);

    private void OnRelease(Vector2 vector)
    {
        if (Hold.Holder is not Player player) return;
        player.Speed = RetentionSpeed;
        if (!noLiftBoost) player.LiftSpeed = RetentionSpeed;
        noLiftBoost = false;
        player.LaunchedBoostCheck();
        NoGrabTimer = GrabCooldown;
    }

    private void OnCarry(Vector2 vector) { }

    private void OnPickup()
    {
        Hold.Holder?.StateMachine.State = Player.StNormal;
        Vector2 holderSpeed = Hold.Holder.Speed;
        if (
            (
                (
                    (Position - End).Length() < 0.5f && Direction.X > 0 ||
                    (Position - Start).Length() < 0.5f && Direction.X < 0
                ) && holderSpeed.X > 0
            ) || (
                (
                    (Position - Start).Length() < 0.5f && Direction.X > 0 ||
                    (Position - End).Length() < 0.5f && Direction.X < 0
                ) && holderSpeed.X < 0
            )
        ) holderSpeed.X = 0;
        if (
            (
                (
                    (Position - End).Length() < 0.5f && Direction.Y > 0 ||
                    (Position - Start).Length() < 0.5f && Direction.Y < 0
                ) && holderSpeed.Y > 0
            ) || (
                (
                    (Position - Start).Length() < 0.5f && Direction.Y > 0 ||
                    (Position - End).Length() < 0.5f && Direction.Y < 0
                ) && holderSpeed.Y < 0
            )
        ) holderSpeed.Y = 0;
        Speed = holderSpeed;
        Hold.Holder.Speed = Vector2.Zero;
        DoGravity = true;
    }

    public override void Update()
    {
        base.Update();
        Collidable = NoGrabTimer <= 0f && !(Scene.Tracker.GetEntity<Player>() is Player pl && (pl.Stamina <= 0f || pl.OnGround()));
        Hold.PickupCollider = Collidable ? Collider : new Hitbox(0, 0, -1e10f, -1e10f);
        NoGrabTimer -= Engine.DeltaTime;
        Hold.Holder?.minHoldTimer = 0f;

        // Make absolutely sure we're still on the track
        if (End == Start)
        {
            Start = End = Position;
        }
        else
        {
            if (DoGravity)
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * Engine.DeltaTime);

            var oldPos = Position;

            // Keep it on track
            float speedFac = Vector2.Dot(Speed.SafeNormalize(), Direction.SafeNormalize());
            Speed = Speed.Length() * Direction * speedFac;
            if (Speed.LengthSquared() >= 1 || (Hold.Holder is not null && Input.MoveX != 0))
                Position += Speed * Engine.DeltaTime;

            float progress = Vector2.Dot(Position - Start, End - Start) / (End - Start).LengthSquared();
            Position = Start + (End - Start) * progress;
            float angleDifference = 1 - Math.Abs(Vector2.Dot((Position - Start).SafeNormalize(), Direction));
            if (progress < 0 || progress > 1)
            {
                if (progress < 0) Position = Start + Direction * 0.1f;
                else Position = End - Direction * 0.1f;
                Vector2 oldSpeed = Speed;
                Speed = Vector2.Zero;
                if (oldSpeed.Length() > HoldSpeedLimit)
                {
                    NoGrabTimer = GrabCooldown;
                    if (Hold.Holder is Player p)
                    {
                        p.Drop();
                        p.jumpGraceTimer = Player.JumpGraceTime;
                    }
                    if (Math.Abs(oldSpeed.X) > TiltSpriteThreshold)
                        Sprite.Play(oldSpeed.X < 0 ? "swingLeft" : "swingRight");
                }
            }
            else
                Speed = (Position - oldPos) / Engine.DeltaTime;
            if (angleDifference > 0.1 && End != Start)
                if (Hold.Holder is Player p) p.Drop();
        }
        if (RetentionSpeed.LengthSquared() > Speed.LengthSquared())
        {
            if (RetentionTimer <= 0f) RetentionTimer = RetentionTime;
            else
            {
                RetentionTimer -= Engine.DeltaTime;
                if (RetentionTimer <= 0f) RetentionSpeed = Speed;
            }
        }
        else
        {
            RetentionSpeed = Speed;
            RetentionTimer = 0f;
        }



        float movementTarget = 0f;
        // Move the player to us
        if (Hold.Holder is Player player)
        {
            player.StateMachine.State = Player.StNormal;
            if (player.Stamina <= 0f) player.Drop();
            else
            {
                if (TakeStamina) player.Stamina -= StaminaCost * Engine.DeltaTime;
                player.Speed = RetentionSpeed;
                bool wasNaive = player.TreatNaive;
                if (Start == End) player.Position = Position + Vector2.UnitY * PlayerOffset;
                else
                {
                    player.MoveToX(Position.X, OnBonkH);
                    player.MoveToY(Position.Y + PlayerOffset, OnBonkV);
                }
                movementTarget = Input.MoveX * PlayerMaxSpeed;
                if (Input.Jump.Pressed)
                {
                    Input.Jump.ConsumePress();
                    noLiftBoost = true;
                    player.Drop();
                    player.Stamina -= JumpStaminaCost;
                    player.Jump(false, true);
                    NoGrabTimer = GrabCooldown;
                }
            }
        }
        if (Sprite.CurrentAnimationID is "idle" or "pushLeft" or "pushRight")
            Sprite.Play(Speed.X > TiltSpriteThreshold ? "pushRight" : Speed.X < -TiltSpriteThreshold ? "pushLeft" : "idle");
        if (Start != End)
        {
            Speed.X = Calc.Approach(Speed.X, movementTarget, Friction * Engine.DeltaTime);
            Speed.Y = Calc.Approach(Speed.Y, 0, Friction * Engine.DeltaTime);
        }

    }

    private void OnBonkH(CollisionData data)
    {
        if (Hold.Holder is not Player player) return;
        NoGrabTimer = GrabCooldown;
        player.Drop();
    }

    private void OnBonkV(CollisionData data)
    {
        if (Hold.Holder is not Player player) return;
        NoGrabTimer = GrabCooldown;
        player.Drop();
    }

    private static readonly float FRAC_SQRT_2_2 = MathF.Sqrt(2) / 2;

    public override void Render()
    {
        DrawRope(ropeSlices, Start - Vector2.UnitX, End - Vector2.UnitX, Color.Black);
        DrawRope(ropeSlices, Start + Vector2.UnitX, End + Vector2.UnitX, Color.Black);
        DrawRope(ropeSlices, Start - Vector2.UnitY, End - Vector2.UnitY, Color.Black);
        DrawRope(ropeSlices, Start + Vector2.UnitY, End + Vector2.UnitY, Color.Black);
        DrawRope(ropeSlices, Start, End, Color.White);
        Sprite.DrawSimpleOutline();
        base.Render();
        tieTexture.DrawCentered(Start);
        tieTexture.DrawCentered(End);
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Line(Start, End, Color.Cyan);
    }

    internal static void DrawRope(MTexture[] ropeSlices, Vector2 start, Vector2 end, Color color)
    {
        if (start == end) return;
        var dir = (end - start).SafeNormalize();
        if (Math.Abs(dir.X) < FRAC_SQRT_2_2)
        {
            // Vertical rope
            for (int y = (int)Math.Min(start.Y, end.Y) + 1; y < (int)Math.Max(start.Y, end.Y); y++)
            {
                int x = (int)(start.X + dir.X / dir.Y * (y - start.Y));
                int index = ((y % ropeSlices.Length) + ropeSlices.Length) % ropeSlices.Length;
                MTexture tex = ropeSlices[index];
                tex.Draw(new(x, y), new(0, tex.Height / 2), color, 1, MathF.PI / 2);
            }
        }
        else
        {
            // Horizontal rope
            for (int x = (int)Math.Min(start.X, end.X) + 1; x < (int)Math.Max(start.X, end.X); x++)
            {
                int y = (int)(start.Y + dir.Y / dir.X * (x - start.X));
                int index = ((x % ropeSlices.Length) + ropeSlices.Length) % ropeSlices.Length;
                MTexture tex = ropeSlices[index];
                tex.Draw(new(x, y), new(0, tex.Height / 2), color);
            }
        }
    }

    [OnLoad]
    internal static void LoadHooks()
    {
        On.Celeste.Player.Throw += OnThrow;
    }


    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Celeste.Player.Throw -= OnThrow;
    }

    private static void OnThrow(On.Celeste.Player.orig_Throw orig, Player self)
    {
        var ent = self.Holding?.Entity;
        Vector2 oldSpeed = self.Speed;
        orig(self);
        Vector2 deltaSpeed = self.Speed - oldSpeed;
        if (ent is HangRail hangrail)
        {
            self.Speed = self.LiftSpeed = hangrail.RetentionSpeed;
            hangrail.Speed.X -= deltaSpeed.X / 2;
            self.Speed.X -= deltaSpeed.X / 2;
        }
    }
}
