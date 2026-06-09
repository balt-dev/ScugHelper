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
            bool res = entity is Player player && ((player.LastBooster?.BoostingPlayer ?? false) ^ field.Invert);
            if (res && field.Destroy)
                (entity as Player)!.StateMachine.State = Player.StNormal;
            field.BouncedBooster |= res;
            return res;
        }
    }

    private bool BouncedBooster;
    private readonly bool Invisible;
    private readonly bool Invert;
    private readonly bool Destroy;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public BoosterField(Vector2 position, float width, float height, bool invis, bool invert, bool destroy) : base(position, width, height, false) {
        Depth = -20000;
        SurfaceSoundIndex = 32;
        Collider = new BoosterFieldColliderList(this);
        Collidable = true;
        Invisible = invis;
        Destroy = destroy;
        Invert = invert;
    }

    public BoosterField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("invisible"), data.Bool("invert"), data.Bool("destroy")) { }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        if (!Invisible) {
            var outlineColor = Invert ? Color.Black : Color.White;
            WobblyHelper.RenderFill((Scene as Level)!.Camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Coral * 0.3f, outlineColor * 0.5f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), outlineColor * (BounceTimer / BouncePulseLength * 0.3f));
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), outlineColor * 0.5f);
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
        if (BouncedBooster) {
            BounceTimer = BouncePulseLength;
            BouncedBooster = false;
        } else {
            BounceTimer = Math.Max(0.0f, BounceTimer - Engine.DeltaTime);
        }
        base.Update();
    }

    public void OnRenderBloom() {
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
