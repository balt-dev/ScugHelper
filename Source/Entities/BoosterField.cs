using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/BoosterField")]
public class BoosterField : Solid
{
    internal class BoosterFieldColliderList(BoosterField self) : AbstractEntityColliderList(self) {
        protected override bool CheckEntity(Entity entity) {
            bool res = entity is Player player && ((player.LastBooster?.BoostingPlayer ?? false) ^ self.Invert);
            if (res && self.Destroy)
                (entity as Player)!.StateMachine.State = Player.StNormal;
            self.BouncedBooster |= res;
            return res;
        }
    }

    private bool BouncedBooster;
    private readonly bool Invisible;
    private readonly bool Invert;
    private readonly bool Destroy;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public BoosterField(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false) {
        Depth = data.Int("Depth", -20000);
        SurfaceSoundIndex = 32;
        Collider = new BoosterFieldColliderList(this);
        Collidable = true;
        Invisible = data.Bool("invisible");
        Destroy = data.Bool("destroy");
        Invert = data.Bool("invert");
    }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        if (!Invisible) {
            Camera camera = (Scene as Level)!.Camera;
            var outlineColor = Invert ? Color.Black : Color.White;
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Coral * 0.3f, outlineColor * 0.5f);
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), outlineColor * (BounceTimer / BouncePulseLength * 0.3f));
            WobblyHelper.RenderOutline(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), outlineColor * 0.5f);
        }

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
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
