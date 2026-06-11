using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/DeSeekerField")]
public class DeSeekerField : Entity
{

    protected float[] speeds = [12f, 20f, 40f];
    private bool BouncedBooster;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public DeSeekerField(Vector2 position, float width, float height) : base(position) {
        Depth = 100;
        Collidable = true;
        Collider = new Hitbox(width, height);
        Add(new SeekerCollider(ScugHelperModule.KillSeeker));
        Add(new PlayerCollider((player) => { if (player.Get<PlayerSeekerComponent>() is var comp) player.Remove(comp); }));
    }

    public DeSeekerField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height) { }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        WobblyHelper.RenderFill((Scene as Level)!.Camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Seeker.TrailColor * 0.3f, Color.White * 0.7f);
        base.Render();
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    private float Elapsed = 0;
    public override void Update() {
        Elapsed += Engine.DeltaTime;
        if (BouncedBooster) {
            BounceTimer = BouncePulseLength;
            BouncedBooster = false;
        } else {
            BounceTimer = Math.Max(0.0f, BounceTimer - Engine.DeltaTime);
        }
        base.Update();
    }

    public void OnRenderBloom() {
        Camera camera = (Scene as Level)!.Camera;
        if (Visible)
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
