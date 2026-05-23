using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RefillField")]
public class RefillField : Entity
{
    public enum RefillType {
        Midair,
        Overcharge,
        Limbo
    }
    
    protected float[] speeds = [12f, 20f, 40f];
    protected List<Vector2> particles = [];
    public bool State { get; protected set; }
    public RefillType Refill { get; protected set; }

    public RefillField(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = 100;
        Collidable = true;
        Refill = data.Enum<RefillType>("RefillType");
        State = data.Bool("State", true);
        if (data.Bool("CoverRoom", false)) {
            Visible = false;
            Position = data.Level.Position;
            Collider = new Hitbox(data.Level.Bounds.Width + 256, data.Level.Bounds.Height + 256, -128, -128);
        } else {
            Collider = new Hitbox(data.Width, data.Height);
            for (int i = 0; i < Width * Height / 24f; i++)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
        }
        Add(new PlayerCollider(OnPlayer));
    }

    private void OnPlayer(Player player) {
        switch (Refill) {
            case RefillType.Midair:
                MidairRefill.MidairDashCount = State ? 1 : 0;
                break;
            case RefillType.Overcharge:
                OverchargeRefill.OverchargeDashCount = State ? 1 : 0;
                break;
            case RefillType.Limbo:
                LimboRefill.LimboTimer = State ? 0.5f : 0f;
                break;
        }
    }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        Color infill = Refill switch {
            RefillType.Midair => Color.Blue,
            RefillType.Overcharge => OverchargeRefill.TrailColor,
            RefillType.Limbo => Color.Black,
        };
        Color partColor = State ? Color.White : Color.Black;
        WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f, infill * 0.3f);
        foreach (Vector2 particle in particles)
            Draw.Pixel.Draw(Position + particle, Vector2.Zero, partColor * 0.7f);

        base.Render();
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    private float Elapsed = 0;
    public override void Update() {
        Elapsed += Engine.DeltaTime;
        int num = speeds.Length;
        float height = Height;
        int i = 0;
        for (int count = particles.Count; i < count; i++) {
            Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
            value.Y %= height - 1f;
            particles[i] = value;
        }
        base.Update();
    }

    public void OnRenderBloom() {
        if (Visible)
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f, Color.White * 0.3f);
    }
}
