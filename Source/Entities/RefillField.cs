using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RefillField")]
public class RefillField : Entity
{
    public enum RefillType {
        Midair,
        Overcharge,
        Limbo
    }

    bool oldState;
    public bool RevertOnLeave { get; protected set; }
    public bool State { get; protected set; }
    public RefillType Refill { get; protected set; }

    public RefillField(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = 100;
        Collidable = true;
        Refill = data.Enum<RefillType>("RefillType");
        State = data.Bool("State", true);
        RevertOnLeave = data.Bool("RevertOnLeave");
        if (data.Bool("CoverRoom", false)) {
            Visible = false;
            Position = data.Level.Position;
            Collider = new Hitbox(data.Level.Bounds.Width + 256, data.Level.Bounds.Height + 256, -128, -128);
        } else {
            Collider = new Hitbox(data.Width, data.Height);
        }
        Add(new PlayerCollider(OnPlayer));
    }

    private void OnPlayer(Player player) {
        switch (Refill) {
            case RefillType.Midair:
                if (RevertOnLeave) oldState = MidairRefill.MidairDashCount > 0;
                MidairRefill.MidairDashCount = State ? 1 : 0;
                break;
            case RefillType.Overcharge:
                if (RevertOnLeave) oldState = OverchargeRefill.OverchargeDashCount > 0;
                OverchargeRefill.OverchargeDashCount = State ? 1 : 0;
                break;
            case RefillType.Limbo:
                if (RevertOnLeave) oldState = LimboRefill.LimboTimer > 0;
                LimboRefill.LimboTimer = State ? 0.5f : 0f;
                break;
        }
    }

    private static readonly float SineMovement = 2.0f;

    public override void Render() {
        Color infill = Refill switch {
            RefillType.Midair => Color.Blue,
            RefillType.Overcharge => OverchargeRefill.TrailColor,
            RefillType.Limbo => Color.Black,
        };
        Color partColor = State ? Color.White : Color.Black;
        WobblyHelper.RenderFill((Scene as Level)!.Camera, Collider.Bounds, Elapsed, SineMovement, 2f, infill * 0.12f, partColor * 0.4f);

        base.Render();
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Add(new Utils.CustomLight(OnRenderLight));
    }

    private float Elapsed = 0;
    public override void Update() {
        Elapsed += Engine.DeltaTime;
        base.Update();
    }

    const int LightPadding = 3;

    public void OnRenderLight() {
        if (Visible)
            Draw.Rect(
                Collider.AbsoluteLeft - LightPadding, Collider.AbsoluteTop - LightPadding,
                Collider.Width + LightPadding * 2, Collider.Height + LightPadding * 2,
                Color.White 
            );
    }
}
