using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/CustomField")]
[Obsolete("Use NewCustomField instead")]
public class CustomField : Solid
{
    internal class CustomFieldColliderList : ColliderList
    {
        private readonly CustomField field;
        public CustomFieldColliderList(CustomField field) {
            colliders = [field.Collider];
            this.field = field;
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity) {
            bool res = field.Types.Contains(entity.GetType().FullName ?? "") ^ field.Invert;
            field.HitEntity |= res;
            return res;
        }
    }

    private static readonly float BouncePulseLength = 0.8f;
    protected static readonly float[] speeds = [12f, 20f, 40f];
    private static readonly float SineMovement = 2.0f;

    internal readonly bool Invert;
    internal readonly bool Invisible;
    internal readonly string[] Types;
    internal readonly Color Color;

    protected List<Vector2> particles = [];
    internal bool HitEntity;
    internal float BounceTimer;

    public CustomField(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, false) {
        Depth = -20000;
        Types = data.String("Types").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Invert = data.Bool("Invert");
        Color = data.HexColor("Color", new(0.7f, 0.85f, 1.0f));
        Collider = new CustomFieldColliderList(this);
        Collidable = true;
        Invisible = data.Bool("Invisible");
        for (int i = 0; i < Width * Height / 24f; i++)
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
    }


    public override void Render() {
        if (!Invisible) {
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color * 0.3f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * (BounceTimer / BouncePulseLength * 0.1f));
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.5f);
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, Color.White * 0.7f);
        }

        base.Render();
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    private float Elapsed = 0;

    public override void Update() {
        Elapsed += Engine.DeltaTime;
        if (HitEntity) {
            BounceTimer = BouncePulseLength;
            HitEntity = false;
        } else {
            BounceTimer = Math.Max(0.0f, BounceTimer - Engine.DeltaTime);
        }
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
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.1f);
    }
}
