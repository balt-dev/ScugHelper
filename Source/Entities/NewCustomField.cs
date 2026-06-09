using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/NewCustomField")]
public class NewCustomField : Solid
{
    internal class CustomFieldColliderList : ColliderList
    {
        private readonly NewCustomField field;
        public CustomFieldColliderList(NewCustomField field) {
            colliders = [field.Collider];
            this.field = field;
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity) {
            var res = entity switch
            {
                Player => field.Names.Contains("player"),
                SolidTiles => field.Names.Contains("fg"),
                BackgroundTiles => field.Names.Contains("bg"),
                _ => field.Names.Intersect(ScugHelperModule.GetNamesOfEntity(entity)).Count() > 0,
            };
            res ^= field.Invert;
            field.HitEntity |= res;
            return res;
        }
    }

    private static readonly float BouncePulseLength = 0.8f;
    private static readonly float SineMovement = 2.0f;

    internal readonly bool Invert;
    internal readonly bool Invisible;
    internal readonly HashSet<string> Names;
    internal readonly Color Color;

    internal bool HitEntity;
    internal float BounceTimer;

    public NewCustomField(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, false) {
        Depth = -20000;
        SurfaceSoundIndex = 32;
        Names = [.. data.String("Names").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        Invert = data.Bool("Invert");
        Color = data.HexColor("Color", new(0.7f, 0.85f, 1.0f));
        Collider = new CustomFieldColliderList(this);
        Collidable = true;
        Invisible = data.Bool("Invisible");
    }


    public override void Render() {
        if (!Invisible) {
            WobblyHelper.RenderFill((Scene as Level)!.Camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color * 0.3f, Color.White * 0.7f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * (BounceTimer / BouncePulseLength * 0.1f));
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.5f);
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
        base.Update();
    }

    public void OnRenderBloom() {
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.1f);
    }
}
