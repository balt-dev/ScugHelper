using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/BoosterField")]
public class BoosterField : Solid
{
    internal class BoosterFieldColliderList : ColliderList
    {
        private readonly BoosterField field;
        public BoosterFieldColliderList(BoosterField field) {
            colliders = [field.Collider];
            this.field = field;
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity) {
            bool res = entity is Player player && (player.LastBooster?.BoostingPlayer ?? false);
            field.BouncedBooster |= res;
            return res;
        }
    }

    protected float[] speeds = [12f, 20f, 40f];
    protected List<Vector2> particles = [];
    private bool BouncedBooster;
    private bool Invisible;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public BoosterField(Vector2 position, float width, float height, bool invis) : base(position, width, height, false)
    {
        Collider = new BoosterFieldColliderList(this);
        Collidable = true;
        Invisible = invis;
        for (int i = 0; i < Width * Height / 24f; i++)
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
    }

    public BoosterField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("invisible"))
    { }

    private static readonly float SineMovement = 2.0f;

    public override void Render()
    {
        if (!Invisible)
        {
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Coral * 0.3f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * (BounceTimer / BouncePulseLength * 0.4f));
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.5f);
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, Color.White * 0.7f);
        }

        base.Render();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    private float Elapsed = 0;
    public override void Update()
    {
        Elapsed += Engine.DeltaTime;
        if (BouncedBooster) {
            BounceTimer = BouncePulseLength;
            BouncedBooster = false;
        } else {
            BounceTimer = Math.Max(0.0f, BounceTimer - Engine.DeltaTime);
        }
        int num = speeds.Length;
        float height = Height;
        int i = 0;
        for (int count = particles.Count; i < count; i++)
        {
            Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
            value.Y %= height - 1f;
            particles[i] = value;
        }
        base.Update();
    }

    public void OnRenderBloom()
    {
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
