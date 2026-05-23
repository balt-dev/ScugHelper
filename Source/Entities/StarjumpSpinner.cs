using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/StarjumpSpinner")]
[Tracked]
public class StarjumpSpinner : Entity
{
    internal readonly int RandomSeed;
    internal readonly int ID;
    internal readonly List<MTexture> bgTex = [];
    internal readonly List<MTexture> fgTex = [];

    public bool AttachToSolid;

    public StarjumpSpinner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        ID = id.ID;
        Depth = -8500;
        AttachToSolid = data.Bool("AttachToSolid");
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        RandomSeed = Calc.Random.Next();
        Add(new PlayerCollider(static player => player.Die(-player.Speed.SafeNormalize(Vector2.UnitY))));
        var spriteDir = data.String("SpritePath", "danger/crystal");
        bgTex = GFX.Game.GetAtlasSubtextures(spriteDir + "/bg_white");
        fgTex = GFX.Game.GetAtlasSubtextures(spriteDir + "/fg_white");
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<StarjumpOutlineRenderer>()?.Track(this);
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        CreateSpinnerSprites(scene);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<StarjumpOutlineRenderer>()?.Untrack(this);
    }

    private void AddFiller(Vector2 offset) {
        Image image = new(Calc.Random.Choose(bgTex)) {
            Position = offset,
            Rotation = Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f),
            Color = Color.Gray
        };
        image.CenterOrigin();

        Add(image);
    }

    private void CreateSpinnerSprites(Scene scene) {
        Calc.PushRandom(RandomSeed);
        foreach (StarjumpSpinner entity in scene.Tracker.GetEntities<StarjumpSpinner>())
            if (entity.ID > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < 576f)
                AddFiller((Position + entity.Position) / 2f - Position);

        MTexture mTexture = Calc.Random.Choose(fgTex);

        if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
            Add(new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f));

        if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
            Add(new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f));

        if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
            Add(new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f));

        if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
            Add(new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f));

        Calc.PopRandom();
    }

    public bool SolidCheck(Vector2 position) {
        if (AttachToSolid) return false;

        foreach (Solid item in Scene.CollideAll<Solid>(position))
            if (item is SolidTiles) return true;

        return false;
    }
}
