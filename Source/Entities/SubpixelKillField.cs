using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SubpixelKillField")]
public class SubpixelKillField : Entity
{

    protected float StartX;
    protected float EndX;
    protected float StartY;
    protected float EndY;

    public SubpixelKillField(EntityData data, Vector2 offset) : base(data.Position + offset) {
        StartX = Math.Clamp(data.Float("StartX", -0.5f), -0.5f, 0.5f);
        EndX = Math.Clamp(data.Float("EndX", 0.5f), -0.5f, 0.5f);
        StartY = Math.Clamp(data.Float("StartY", -0.5f), -0.5f, 0.5f);
        EndY = Math.Clamp(data.Float("EndY", 0.5f), -0.5f, 0.5f);
        Depth = 100;
        Collidable = true;
        Collider = new Hitbox(Math.Max(data.Width, 2f), Math.Max(data.Height, 2f));
        var innerCollider = new Hitbox(Math.Max(data.Width, 2f) - 2, Math.Max(data.Height, 2f) - 2, 1, 1);
        Add(new PlayerCollider(OnPlayer));
        Add(new PlayerCollider((player) => player.Die(Vector2.Zero), innerCollider));
    }

    private void OnPlayer(Player player) {
        float approxXOffset = player.movementCounter.X - Position.X;
        float approxYOffset = player.movementCounter.Y - Position.Y;
        float approxHCenter = player.Center.X + approxXOffset;
        float approxVCenter = player.Center.Y + approxYOffset;
        if (approxHCenter < Width / 2 && player.movementCounter.X < StartX) return;
        if (approxHCenter >= Width / 2 && player.movementCounter.X > EndX) return;
        if (approxVCenter < Height / 2 && player.movementCounter.Y < StartY) return;
        if (approxVCenter >= Height / 2 && player.movementCounter.Y > EndY) return;
        player.Die(Vector2.Zero);
    }
}
