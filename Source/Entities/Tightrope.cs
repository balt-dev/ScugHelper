
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
public class Tightrope : Solid
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
            if (entity is not Actor actor) return false;
            if (!tightrope.IsRiding(actor)) return false;
            if (entity is not Player player) return true;
            if (!player.wasOnGround) return false;
            if (player.Ducking) return false;
            return true;
        }
    }

    private readonly MTexture[] ropeSlices;
    private readonly MTexture tieTexture;
    private readonly TightropeColliderList colliderList;

    public Tightrope(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, 2f, true)
    {
        Depth = 1000;
        SurfaceSoundIndex = 17;
        Collider = new Hitbox(data.Width, 2f);
        Collider = colliderList = new TightropeColliderList(this);
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

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
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

    public override void MoveHExact(int move)
    {
        if (Collidable)
        {
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
            {
                if (!IsRiding(actor)) continue;
                if (actor.TreatNaive) actor.NaiveMove(Vector2.UnitX * move);
                else actor.MoveHExact(move);
            }
        }
        X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }

    public override void MoveVExact(int move)
    {
        if (Collidable)
        {
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
            {
                if (!IsRiding(actor)) continue;
                Collidable = false;
                if (actor.TreatNaive) actor.NaiveMove(Vector2.UnitY * move);
                else actor.MoveVExact(move);
                actor.LiftSpeed = LiftSpeed;
                Collidable = true;
            }
        }

        Y += move;
        MoveStaticMovers(Vector2.UnitY * move);
    }

    private bool IsRiding(Actor actor)
    {
        if (actor.IgnoreJumpThrus) return false;
        if (SpeedAccessor.For(actor) is not SpeedAccessor accessor) return false;
        Collider = colliderList.colliders[0];
        var res = accessor.Speed.Y == 0 && actor.OnGround() && (Math.Abs(actor.IsInverted() ? (actor.Top - Bottom) : (actor.Bottom - Top)) < 2);
        Collider = colliderList;
        return res;
    }
}
