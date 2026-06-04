using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

#nullable enable
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/NewAccelerationField")]
public class NewAccelerationField : Entity
{
    public enum AccelerationBehavior {
        Always,
        Never,
        LessThanVelocity,
        GreaterThanVelocity,
        LessThanMagnitude,
        GreaterThanMagnitude,
    }
    
    public readonly Vector2 SpeedTarget;
    public readonly Vector2 SpeedApproach;
    public readonly AccelerationBehavior BehaviorX;
    public readonly AccelerationBehavior BehaviorY;
    public readonly bool Everywhere;
    public readonly Color FieldColor;
    public readonly Color OutlineColor;
    public readonly bool DrawParticles;

    private readonly List<FieldParticle> FieldParticles = [];

    public NewAccelerationField(EntityData data, Vector2 offset)
     : base(data.Position + offset)
    
    {
        Depth = -10000;
        Add(new CustomBloom(OnRenderBloom));
        Everywhere = data.Bool("Everywhere", false);
        Collider = Everywhere ? new Hitbox(1e20f, 1e20f, -1e19f, -1e19f) : new Hitbox(data.Width, data.Height);
        DrawParticles = data.Bool("DrawParticles", true) && !Everywhere;
        FieldColor = data.HexColor("FieldColor", Color.LightBlue) * data.Float("FieldOpacity", 0.1f);
        OutlineColor = data.HexColor("OutlineColor", Color.AliceBlue) * data.Float("FieldOpacity", 0.2f);
        SpeedTarget = new(data.Float("TargetX", 0), data.Float("TargetY", 0));
        SpeedApproach = new(data.Float("AccelX", 0), data.Float("AccelY", 0));
        BehaviorX = data.Enum("BehaviorX", AccelerationBehavior.Always);
        BehaviorY = data.Enum("BehaviorY", AccelerationBehavior.Always);
        if (DrawParticles)
            for (int i = 0; i < Math.Max(1, Width * Height / 64); i++)
                FieldParticles.Add(new(Calc.Random.Range(TopLeft + Vector2.One, BottomRight - Vector2.One)));
    }

    private float Elapsed = 0;
    public override void Update() {
        base.Update();
        Elapsed += Engine.DeltaTime;
        
        if (DrawParticles)
            for (int i = 0; i < FieldParticles.Count; i++)
                if (!FieldParticles[i].Step(this))
                    FieldParticles[i] = new(Calc.Random.Range(TopLeft, BottomRight));
    }

    private void AccelerateSpeed(ref Vector2 oldSpeed) {
        if (BehaviorX switch {
            AccelerationBehavior.Never => false,
            AccelerationBehavior.Always => true,
            AccelerationBehavior.LessThanVelocity => oldSpeed.X < SpeedTarget.X,
            AccelerationBehavior.GreaterThanVelocity => oldSpeed.X > SpeedTarget.X,
            AccelerationBehavior.LessThanMagnitude => Math.Abs(oldSpeed.X) < Math.Abs(SpeedTarget.X),
            AccelerationBehavior.GreaterThanMagnitude => Math.Abs(oldSpeed.X) > Math.Abs(SpeedTarget.X),
        }) oldSpeed.X = Calc.Approach(oldSpeed.X, SpeedTarget.X, SpeedApproach.X * Engine.DeltaTime);
        if (BehaviorY switch {
            AccelerationBehavior.Never => false,
            AccelerationBehavior.Always => true,
            AccelerationBehavior.LessThanVelocity => oldSpeed.Y < SpeedTarget.Y,
            AccelerationBehavior.GreaterThanVelocity => oldSpeed.Y > SpeedTarget.Y,
            AccelerationBehavior.LessThanMagnitude => Math.Abs(oldSpeed.Y) < Math.Abs(SpeedTarget.Y),
            AccelerationBehavior.GreaterThanMagnitude => Math.Abs(oldSpeed.Y) > Math.Abs(SpeedTarget.Y),
        }) oldSpeed.Y = Calc.Approach(oldSpeed.Y, SpeedTarget.Y, SpeedApproach.Y * Engine.DeltaTime);
    }

    private static readonly float SineMovement = -2.0f;

    public override void Render() {
        base.Render();
        if (Everywhere) return;

        WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2, FieldColor);
        WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2, OutlineColor);
        
        if (DrawParticles)
            foreach (FieldParticle particle in FieldParticles)
                particle.Render();
    }

    public void OnRenderBloom() {
        if (!Everywhere) WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2, Color.White * 0.3f);
    }
    
    internal class FieldParticle(Vector2 pos)
    {
        private Vector2 Position = pos;
        private Vector2 Speed = Vector2.Zero;
        private float Elapsed = 0;
        private readonly float Lifetime = Calc.Random.Range(0.3f, 0.6f);
        private static readonly Color ParticleColor = Color.AliceBlue;
    
        internal bool Step(NewAccelerationField field) {
            field.AccelerateSpeed(ref Speed);
            Position += Speed * Engine.DeltaTime;
            if (!field.Collider.Bounds.Contains((int)Position.X, (int)Position.Y)) {
                Speed = Vector2.Zero;
                if (Position.X < field.Left + 1) Position.X = field.Right - 1;
                if (Position.X > field.Right - 1) Position.X = field.Left + 1;
                if (Position.Y < field.Top + 1) Position.Y = field.Bottom - 1;
                if (Position.Y > field.Bottom - 1) Position.Y = field.Top + 1;
            }
            Elapsed += Engine.DeltaTime;
            return Elapsed <= Lifetime;
        }
        internal void Render() {
            Draw.Point(Position, Color.Lerp(ParticleColor, Color.Transparent, Elapsed / Lifetime));
        }
    }
    
    [OnLoad]
    internal static void LoadHooks() => On.Monocle.Entity.Added += OnEntityAdded;
    
    private static void OnEntityAdded(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene) {
        orig(self, scene);
        if (self is SolidTiles) return;
        if (self is Decal) return;
        if (SpeedAccessor.For(self) is SpeedAccessor accessor)
            self.Add(new AccelerationFieldCollider(accessor));
    }

    private class AccelerationFieldCollider(SpeedAccessor accessor) : Component(true, false) {
        public override void Update() {
            var spd = accessor.Speed;
            foreach (NewAccelerationField field in Entity.CollideAll<NewAccelerationField>()) field.AccelerateSpeed(ref spd);
            accessor.Speed = spd;
        }
    }
}

#nullable restore
