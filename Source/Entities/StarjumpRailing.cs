using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections.Generic;
using MonoMod.Cil;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/StarjumpRailing")]
public class StarjumpRailing : Entity
{
    private Vector2 imageOffset = Vector2.Zero;

    public StarjumpRailing(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = data.Int("Depth", -10);
        Collider = new Hitbox(data.Width, 8, 0, -8);
        Collidable = false;
        Add(new StaticMover
        {
            SolidChecker = IsRiding,
            OnShake = OnShake,
            OnEnable = OnEnable,
            OnDisable = OnDisable
        });
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        List<MTexture> leftTextures = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/leftrailing");
        List<MTexture> middleTextures = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/railing");
        List<MTexture> rightTextures = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/rightrailing");
        static MTexture wrap(float x, List<MTexture> tex) => tex[(((int)(x / 8) % tex.Count) + tex.Count) % tex.Count];
        if (Width < 8) {
            Image middle = new(wrap(X, middleTextures)) { Position = new Vector2(0, -8f) };
            Add(middle);
            return;
        }
        Image left = new(wrap(X, leftTextures)) { Position = new Vector2(0, -8f) };
        Add(left);
        for (int i = 8; i < Width - 8; i += 8) {
            Image middle = new(wrap(X + i * 8, middleTextures)) { Position = new Vector2(i, -8f) };
            Add(middle);
        }
        Image right = new(wrap(X + Width - 8, rightTextures)) { Position = new Vector2(Width - 8, -8f) };
        Add(right);
    }
    public bool IsRiding(Solid solid) => CollideCheckOutside(solid, Position + Vector2.UnitY);

    public void OnEnable()
    {
        Active = Visible = Collidable = true;
    }

    public void OnDisable()
    {
        Active = Collidable = false;
        Visible = false;
    }
    public void OnShake(Vector2 amount)
    {
        imageOffset += amount;
    }
    public override void Render()
    {
        Vector2 position = Position;
        Position += imageOffset;
        base.Render();
        Position = position;
    }
}
