using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;
using System.Collections;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Reflection;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/LimboField")]
public class LimboField : Solid
{
    internal class LimboFieldColliderList : ColliderList {
        private readonly LimboField field;
        public LimboFieldColliderList(LimboField field) {
            colliders = [field.Collider];
            this.field = field;
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity) {
            bool res = entity is Player && LimboRefill.LimboTimer > 0;
            field.HitPlayer |= res;
            return res;
        }
    }

    protected float[] speeds = [12f, 20f, 40f];
    protected List<Vector2> particles = [];
    private bool HitPlayer;
    private readonly bool Invisible;
    private readonly bool Invert;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public LimboField(Vector2 position, float width, float height, bool invis) : base(position, width, height, false) {
        Depth = -20000;
        Collider = new LimboFieldColliderList(this);
        Collidable = true;
        Invisible = invis;
        for (int i = 0; i < Width * Height / 24f; i++)
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
    }

    public LimboField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("invisible")) { }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        if (!Invisible) {
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Black * 0.3f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Black * (BounceTimer / BouncePulseLength * 0.4f));
            var outlineColor = Color.Black;
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), outlineColor * 0.5f);
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, outlineColor * 0.7f);
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
        if (HitPlayer) {
            BounceTimer = BouncePulseLength;
            HitPlayer = false;
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
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
#nullable restore
