
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Tightrope")]
public class Tightrope : JumpThru
{
    internal class TightropeColliderList : ColliderList
    {
        private readonly Tightrope tightrope;
        public TightropeColliderList(Tightrope tightrope)
        {
            colliders = [tightrope.Collider];
            this.tightrope = tightrope;
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity)
        {
            if (SpeedAccessor.For(entity) is not SpeedAccessor accessor) return false;
            var approxGrounded = Math.Round(accessor.Speed.Y) == 0 && Math.Abs(entity.Bottom - tightrope.Y) < 2;
            if (!approxGrounded) return false;
            if (entity is not Player player) return true;
            if (!player.wasOnGround) return false;
            if (player.Ducking) return false;
            return true;
        }
    }

    private readonly MTexture[] ropeSlices;
    private readonly MTexture tieTexture;

    public Tightrope(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, true)
    {
        Depth = 1000;
        SurfaceSoundIndex = 17;
        Collider = new Hitbox(data.Width, 5f);
        Collider = new TightropeColliderList(this);
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

    public override void Render()
    {
        var start = Position + Vector2.UnitY * 1;
        var end = start + Vector2.UnitX * Width;
        HangRail.DrawRope(ropeSlices, start + Vector2.UnitX, end + Vector2.UnitX, Color.Black);
        HangRail.DrawRope(ropeSlices, start - Vector2.UnitX, end - Vector2.UnitX, Color.Black);
        HangRail.DrawRope(ropeSlices, start + Vector2.UnitY, end + Vector2.UnitY, Color.Black);
        HangRail.DrawRope(ropeSlices, start - Vector2.UnitY, end - Vector2.UnitY, Color.Black);
        HangRail.DrawRope(ropeSlices, start, end, Color.White);
        base.Render();
        tieTexture.DrawCentered(start);
        tieTexture.DrawCentered(end);
    }
}
