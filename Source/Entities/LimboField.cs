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

    private bool HitPlayer;
    private readonly bool Invisible;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public LimboField(Vector2 position, float width, float height, bool invis) : base(position, width, height, false) {
        Depth = -20000;
        Collider = new LimboFieldColliderList(this);
        Collidable = true;
        Invisible = invis;
    }

    public LimboField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("invisible")) { }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        if (!Invisible) {
            Camera camera = (Scene as Level)!.Camera;
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Black * 0.3f, Color.Black * 0.7f);
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Black * (BounceTimer / BouncePulseLength * 0.4f));
            WobblyHelper.RenderOutline(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Black * 0.5f);
        }

        base.Render();
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
        base.Update();
    }
}
#nullable restore
