using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/SeekerRotateSpinner")]
[Tracked]
public class SeekerRotateSpinner : RotateSpinner
{
    internal readonly MTexture Texture;
    internal readonly float Period;

    public SeekerRotateSpinner(EntityData data, Vector2 offset) : base(data, offset)
    {
        Depth = -8500;
        Period = data.Float("Period", 1.8f);
        Add(new PlayerCollider(OnPlayer));
        Add(new SeekerCollider(ScugHelperModule.KillSeeker));
        Texture = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/ScugHelper/seekerSpinner/fg"));
    }

    public override void Render()
    {
        base.Render();
        Texture.DrawCentered(Position);
    }

    public override void Update() {
        var oldMoving = Moving;
        Moving = false;
        base.Update();
        Moving = oldMoving;
        
        if (Moving)
        {
            rotationPercent += Engine.DeltaTime / Period;
            rotationPercent %= 1f;
            Position = center + Calc.AngleToVector(Angle, length);
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Track(this);
    }
    
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        scene.Tracker.GetEntity<SeekerBarrierMaskRenderer>()?.Untrack(this);
    }

    private new void OnPlayer(Player player)
    {
        if (player.Get<PlayerSeekerComponent>() is PlayerSeekerComponent comp)
        {
            comp.disableDeath = false;
            player.Die(-player.Speed.SafeNormalize(Vector2.UnitY));
        }
    }
}
