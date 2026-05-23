using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/SeekerSpinner")]
[Tracked]
public class SeekerSpinner : Entity
{
    internal readonly int RandomSeed;
    internal readonly int ID;
    public bool AttachToSolid;

    public SeekerSpinner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        ID = id.ID;
        Depth = -8500;
        AttachToSolid = data.Bool("AttachToSolid");
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        RandomSeed = Calc.Random.Next();
        Add(new PlayerCollider(OnPlayer));
        Add(new SeekerCollider(ScugHelperModule.KillSeeker));
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Track(this);
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        CreateSpinnerSprites(scene);
    }
    
    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Untrack(this);
    }

    List<MTexture> bgTex = GFX.Game.GetAtlasSubtextures("objects/ScugHelper/seekerSpinner/bg");
    List<MTexture> fgTex = GFX.Game.GetAtlasSubtextures("objects/ScugHelper/seekerSpinner/fg");

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
        // This is the vanilla implementation. O(n^2). Jesus.
        foreach (SeekerSpinner entity in scene.Tracker.GetEntities<SeekerSpinner>())
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

    private void OnPlayer(Player player) {
        if (player.Get<PlayerSeekerComponent>() is PlayerSeekerComponent comp) {
            comp.disableDeath = false;
            player.Die(-player.Speed.SafeNormalize(Vector2.UnitY));
        }
    }

    public bool SolidCheck(Vector2 position) {
        if (AttachToSolid) return false;

        foreach (Solid item in Scene.CollideAll<Solid>(position))
            if (item is SolidTiles) return true;

        return false;
    }
}
