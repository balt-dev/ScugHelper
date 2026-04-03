using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;

[CustomEntity("ScugHelper/SpeedcheckGate")]
public class SpeedcheckGate(EntityData data, Vector2 offset) : AbstractGate(data, offset)
{
    public enum SpeedAxis
    {
        Horizontal,
        Vertical,
        Total
    }
    public enum ComparisonType
    {
        Less,
        LessEq,
        Greater,
        GreaterEq
    }
    public enum TriggerAction
    {
        Kill,
        SetFlag,
        SetSpeed
    }
    
    public SpeedAxis Axis = data.Enum<SpeedAxis>("Axis");
    public ComparisonType Comparison = data.Enum<ComparisonType>("Comparison");
    public TriggerAction Action = data.Enum<TriggerAction>("Action");
    public float Threshold = data.Float("Threshold");

    public string FlagName = data.String("FlagName");
    public Vector2 SetSpeed = new(data.Float("SetX"), data.Float("SetY"));

    public override void OnTrigger(Player player)
    {
        float checkSpeed = Axis switch
        {
            SpeedAxis.Horizontal => Math.Abs(player.Speed.X),
            SpeedAxis.Vertical => Math.Abs(player.Speed.Y),
            SpeedAxis.Total => player.Speed.Length(),
        };
        bool isTriggered = Comparison switch
        {
            ComparisonType.Less => checkSpeed < Threshold,
            ComparisonType.LessEq => checkSpeed <= Threshold,
            ComparisonType.Greater => checkSpeed > Threshold,
            ComparisonType.GreaterEq => checkSpeed >= Threshold
        };
        if (!isTriggered) return;
        switch (Action)
        {
            case TriggerAction.Kill:
                player.Die(Vector2.Zero);
                break;
            case TriggerAction.SetFlag:
                SceneAs<Level>().Session.SetFlag(FlagName);
                break;
            case TriggerAction.SetSpeed:
                player.Speed = SetSpeed;
                break;
        }
    }
}
