using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections.Generic;

#nullable enable
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/AccelerationField")]
public class AccelerationField : Entity
{
    public Vector2 Acceleration;
    public float Drag;
    public bool Everywhere;
    private readonly List<FieldParticle> FieldParticles = [];

    public AccelerationField(EntityData data, Vector2 offset)
     : base(data.Position + offset) {
        Depth = -10000;
        Add(new CustomBloom(OnRenderBloom));
        Collider = new Hitbox(data.Width, data.Height);
        Everywhere = data.Bool("Everywhere", false);
        Acceleration = new(data.Float("AccelX", 0), data.Float("AccelY", 0));
        Drag = data.Float("Drag", 0);
        for (int i = 0; i < Math.Max(1, Width * Height / 64); i++)
            FieldParticles.Add(new(Calc.Random.Range(TopLeft, BottomRight)));
    }

    private float Elapsed = 0;
    public override void Update() {
        base.Update();
        Elapsed += Engine.DeltaTime;

        for (int i = 0; i < FieldParticles.Count; i++)
            if (!FieldParticles[i].Step(this))
                FieldParticles[i] = new(Calc.Random.Range(TopLeft, BottomRight));

        foreach (var kvp in Scene.Tracker.Entities) {
            foreach (Entity entity in kvp.Value) {
                if (entity is SolidTiles) break;
                if (entity is Decal) break;
                if (!Everywhere && !CollideCheck(entity)) continue;
                if (SpeedAccessor.For(entity) is not SpeedAccessor accessor) break;
                accessor.Speed *= MathF.Pow(1f - Drag, Engine.DeltaTime);
                accessor.Speed += Acceleration * Engine.DeltaTime;
            }
        }
    }

    private static readonly Color FieldColor = Color.LightBlue * 0.1f;
    private static readonly Color FieldOutlineColor = Color.AliceBlue * 0.2f;
    private static readonly float SineMovement = -2.0f;

    public override void Render() {
        base.Render();
        if (Everywhere) return;

        WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2, FieldColor);
        WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2, FieldOutlineColor);

        foreach (FieldParticle particle in FieldParticles)
            particle.Render();
    }

    public void OnRenderBloom() {
        if (!Everywhere) WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2, Color.White * 0.3f);
    }
}

internal class FieldParticle(Vector2 pos)
{
    private Vector2 Position = pos;
    private Vector2 Speed = Vector2.Zero;
    private float Elapsed = 0;
    private readonly float Lifetime = Calc.Random.Range(0.3f, 0.6f);
    private static readonly Color ParticleColor = Color.AliceBlue;

    internal bool Step(AccelerationField field) {
        Speed = Calc.Approach(Speed, Vector2.Zero, field.Drag * Engine.DeltaTime);
        Speed += field.Acceleration * Engine.DeltaTime;
        Position += Speed * Engine.DeltaTime;
        if (!field.Collider.Bounds.Contains((int)Position.X, (int)Position.Y)) {
            Speed = Vector2.Zero;
            if (Position.X < field.Left) Position.X = field.Right;
            if (Position.X > field.Right) Position.X = field.Left;
            if (Position.Y < field.Top) Position.Y = field.Bottom;
            if (Position.Y > field.Bottom) Position.Y = field.Top;
        }
        Elapsed += Engine.DeltaTime;
        return Elapsed <= Lifetime;
    }
    internal void Render() {
        Draw.Point(Position, Color.Lerp(ParticleColor, Color.Transparent, Elapsed / Lifetime));
    }
}

#nullable restore
