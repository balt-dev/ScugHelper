
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Conveyor")]
public class Conveyor : Entity
{
    public readonly string SpritePath;
    public readonly float TargetSpeed;
    public readonly bool Flip;
    public readonly float Acceleration;
    public readonly float SpriteRate;

    private readonly List<Sprite> tiles;
    private readonly SoundSource idleSfx;
    private readonly StaticMover staticMover;
    private float elapsed;
    private int TotalFrames = 1;
    private Vector2 imageOffset;

    public Conveyor(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Tag = Tags.TransitionUpdate;
        Depth = 1999;
        Flip = data.Bool("Flip");
        Collider = new Hitbox(data.Width, 2f, 0, Flip ? 0f : 6f);
        Add(staticMover = new StaticMover {
            SolidChecker = IsRiding,
            OnShake = OnShake,
            OnEnable = OnEnable,
            OnDisable = OnDisable
        });
        Add(idleSfx = new SoundSource());
        idleSfx.Play("event:/env/local/09_core/conveyor_idle");
        SpritePath = data.String("SpritePath", "objects/ScugHelper/conveyor");
        TargetSpeed = data.Float("TargetSpeed", 120);
        SpriteRate = TargetSpeed / 60f;
        tiles = BuildTiles();
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

    public List<Sprite> BuildTiles() {
        List<Sprite> list = [];
        for (int i = 0; i < Width; i += 4) {
            Sprite sprite = new(GFX.Game, SpritePath + (i == Width - 4 ? "/right" : i == 0 ? "/left" : "/middle")) { Position = new Vector2(i, Flip ? 0 : 4), Origin = Vector2.Zero, FlipY = Flip };
            sprite.AddLoop("idle", "", 1f / 60f);
            sprite.Play("idle");
            TotalFrames = sprite.CurrentAnimationTotalFrames;
            list.Add(sprite);
            Add(sprite);
        }

        return list;
    }


    public override void Update() {
        PositionIdleSfx();
        if (SceneAs<Level>().Transitioning) return;
        base.Update();
        elapsed += Engine.DeltaTime;
        foreach (Sprite tile in tiles)
            tile.SetAnimationFrame((((int)(elapsed * TargetSpeed) % TotalFrames) + TotalFrames) % TotalFrames);

        foreach (var kvp in Scene.Tracker.Entities) {
            foreach (Entity entity in kvp.Value) {
                if (entity is not Actor actor) break;
                if (!CollideCheck(entity)) continue;
                if (!actor.OnGround()) continue;
                actor.MoveH(TargetSpeed * Engine.DeltaTime / 2); // /2 for some reason?
                actor.LiftSpeed = new(TargetSpeed, actor.LiftSpeed.Y);
            }
        }
    }
    public void PositionIdleSfx() {
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player != null) {
            idleSfx.Position = Calc.ClosestPointOnLine(Position, Position + new Vector2(Width, 0f), player.Center) - Position;
            idleSfx.UpdateSfxPosition();
        }
    }
    public override void Render() {
        Position += imageOffset;
        base.Render();
        Position -= imageOffset;
    }
}
