using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/FlagField")]
public class FlagField : Solid
{
    internal class FlagFieldColliderList : ColliderList {
        private readonly FlagField field;
        public FlagFieldColliderList(FlagField field) {
            colliders = [field.Collider];
            this.field = field;
        }
        public override bool Collide(Circle o) => base.Collide(o) && field.BeginPulse();
        public override bool Collide(Hitbox o) => base.Collide(o) && field.BeginPulse();
        public override bool Collide(ColliderList o) => base.Collide(o) && field.BeginPulse();
        public override bool Collide(Grid o) => base.Collide(o) && field.BeginPulse();
    }

    private static readonly float CollidePulseLength = 0.8f;
    private static readonly float SineMovement = 2.0f;
    private static readonly float TangibleOpacity = 1f;
    private static readonly float IntangibleOpacity = 0.3f;
    private static readonly float TangibleSpeed = 1f;
    private static readonly float IntangibleSpeed = 0.6f;

    public readonly bool Invert;
    public readonly bool Invisible;
    public readonly Color Color;
    public readonly string Flag;
    public readonly bool FlagState;
    internal float FillMultiplier;
    internal float ParticleSpeed;
    internal float CollideTimer;

    public FlagField(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, false) {
        Depth = data.Int("Depth", -20000);
        SurfaceSoundIndex = 32;
        Flag = data.String("Flag");
        FlagState = data.Bool("FlagState", true);
        Color = data.HexColor("Color", new(0.7f, 0.85f, 1.0f));
        Collider = new FlagFieldColliderList(this);
        Invisible = data.Bool("Invisible");
        Add(new CustomBloom(OnRenderBloom));
    }
    
    private bool BeginPulse() {
        CollideTimer = CollidePulseLength;
        return true;
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Collidable = ShouldCollide(scene as Level);
        FillMultiplier = Collidable ? TangibleOpacity : IntangibleOpacity;
        ParticleSpeed = Collidable ? TangibleSpeed : IntangibleSpeed;
    }

    public override void Render() {
        if (!Invisible) {
            Camera camera = (Scene as Level)!.Camera;
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (CollideTimer / CollidePulseLength)), Color * 0.3f * FillMultiplier * FillMultiplier, Color.White * 0.7f * FillMultiplier);
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (CollideTimer / CollidePulseLength)), Color.White * (CollideTimer / CollidePulseLength * 0.1f) * FillMultiplier);
            WobblyHelper.RenderOutline(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (CollideTimer / CollidePulseLength)), Color.White * 0.5f * FillMultiplier);
        }

        base.Render();
    }

    private float Elapsed = 0;
    
    public bool ShouldCollide(Level? level = null) => (level ?? SceneAs<Level>())?.Session.GetFlag(Flag) == FlagState;

    public override void Update() {
        Collidable = ShouldCollide();
        FillMultiplier = Calc.Approach(FillMultiplier, Collidable ? TangibleOpacity : IntangibleOpacity, Engine.DeltaTime / 0.1f);
        ParticleSpeed = Calc.Approach(ParticleSpeed, Collidable ? TangibleSpeed : IntangibleSpeed, Engine.DeltaTime / 0.35f);
        Elapsed += Engine.DeltaTime * ParticleSpeed;
        CollideTimer = Math.Max(0.0f, CollideTimer - Engine.DeltaTime);
        base.Update();
    }

    public void OnRenderBloom() {
        Camera camera = (Scene as Level)!.Camera;
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (CollideTimer / CollidePulseLength)), Color.White * 0.1f);
    }
}
