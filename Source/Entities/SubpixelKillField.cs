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
        Add(new PlayerCollider(OnPlayer));
    }

    private void OnPlayer(Player player) {
        if (player.movementCounter.X > StartX) return;
        if (player.movementCounter.X < EndX) return;
        if (player.movementCounter.Y > StartY) return;
        if (player.movementCounter.Y < EndY) return;
        player.Die(Vector2.Zero);
    }
}
