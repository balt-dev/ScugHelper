
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Kickpad")]
public class Kickpad : Entity
{
    public readonly string SpritePath;
    public readonly bool Flip;

    private readonly List<Image> tiles;
    private readonly StaticMover staticMover;
    private Vector2 imageOffset;

    public Kickpad(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = 1999;
        Flip = data.Bool("Flip");
        Collider = new Hitbox(data.Width, 2f, 0, Flip ? 0 : 6f);
        Add(staticMover = new StaticMover {
            SolidChecker = IsRiding,
            OnShake = OnShake,
            OnEnable = OnEnable,
            OnDisable = OnDisable
        });
        SpritePath = data.String("SpritePath", "objects/ScugHelper/kickpad");
        tiles = BuildTiles();
    }

    public List<Image> BuildTiles() {
        List<Image> list = [];
        for (int i = 0; i < Width; i += 4) {
            Image im = new(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(SpritePath + (i == Width - 4 ? "/right" : i == 0 ? "/left" : "/middle")))) { Position = new Vector2(i, Flip ? 0 : 4), Origin = Vector2.Zero, FlipY = Flip };
            list.Add(im);
            Add(im);
        }

        return list;
    }

    public bool IsRiding(Solid solid) => Flip ? CollideCheckOutside(solid, Position - Vector2.UnitY) : CollideCheckOutside(solid, Position + Vector2.UnitY);

    public void OnEnable() {
        Active = Visible = Collidable = true;
    }
    
    public void OnDisable() {
        Active = Collidable = false;
        Visible = false;
    }
    public void OnShake(Vector2 amount) {
        imageOffset += amount;
    }


    public override void Update() {
        if (SceneAs<Level>().Transitioning) return;
        base.Update();
        if (CollideFirst<Player>() is not Player player) return;
        if (!player.OnGround()) return;
        player.Speed.X = Math.Abs(player.Speed.X) * (int) player.Facing;
    }
    public override void Render() {
        Position += imageOffset;
        base.Render();
        Position -= imageOffset;
    }
}
