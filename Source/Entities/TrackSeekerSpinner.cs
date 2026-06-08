using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using MonoMod;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/SeekerTrackSpinner")]
[Tracked]
public class SeekerTrackSpinner : TrackSpinner
{
    internal readonly MTexture Texture;
    internal readonly float MoveTime;
    internal readonly float PauseTime;

    [MonoModLinkTo("Monocle.Entity", "System.Void Update")]
    private void EntityUpdate() { }

    public SeekerTrackSpinner(EntityData data, Vector2 offset) : base(data, offset) {
        Depth = -8500;
        MoveTime = data.Float("MoveTime", 0.9f);
        PauseTime = data.Float("PauseTime", 0.3f);
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Components.RemoveAll<PlayerCollider>();
        Add(new PlayerCollider(OnPlayer));
        Add(new SeekerCollider(ScugHelperModule.KillSeeker));
        Texture = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/ScugHelper/seekerSpinner/fg"));
    }

    public override void Render() {
        base.Render();
        Texture.DrawCentered(Position);
    }

    public override void Update() {
        EntityUpdate();

        if (!Moving)
            return;

        if (PauseTimer > 0f) {
            PauseTimer -= Engine.DeltaTime;
            if (PauseTimer <= 0f)
                OnTrackStart();

            return;
        }

        Percent = Calc.Approach(Percent, Up ? 1 : 0, Engine.DeltaTime / MoveTime);
        UpdatePosition();
        if ((Up && Percent == 1f) || (!Up && Percent == 0f)) {
            Up = !Up;
            PauseTimer = PauseTime;
            OnTrackEnd();
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Track(this);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Untrack(this);
    }

    private new void OnPlayer(Player player) {
        if (player.Get<PlayerSeekerComponent>() is PlayerSeekerComponent comp) {
            comp.disableDeath = false;
            player.Die(-player.Speed.SafeNormalize(Vector2.UnitY));
        }
    }
}
