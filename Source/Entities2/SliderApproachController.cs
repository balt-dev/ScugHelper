using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/SliderApproachController")]
public class SliderApproachController(EntityData data, Vector2 _) : Entity() {
    internal readonly string Slider = data.String("TargetSlider", "");
    internal readonly string Flag = data.String("DriverFlag", "");
    internal readonly bool State = data.Bool("DriverState", true);
    internal readonly float Value = data.Float("Value", 1f);
    internal readonly float ApproachSpeed = data.Float("ApproachSpeed", 1f);
    
    public override void Update() {
        base.Update();
        if (Scene is not Level level) return;
        if (level.Session.GetFlag(Flag) == State)
            level.Session.SetSlider(
                Slider, Calc.Approach(
                    level.Session.GetSlider(Slider), 
                    Value,
                    ApproachSpeed * Engine.DeltaTime
                )
            );
    }
}