using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;

[CustomEntity("ScugHelper/SpeedcheckGate")]
[Tracked(false)]
public class SpeedcheckGate : Entity
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
    public float Angle;
    public float Size;
    
    public SpeedAxis Axis;
    public ComparisonType Comparison;
    public TriggerAction Action;
    public float Threshold;

    public string FlagName;
    public Vector2 SetSpeed;
    
    private Vector2 lineDir;
    private Vector2 lineNorm;
    
    public SpeedcheckGate(EntityData data, Vector2 offset): base(data.Position + offset) {
        Angle = (float) (data.Float("Angle") * Math.PI / 180);
        Size = data.Float("Size");
        Threshold = data.Float("Threshold");
        Axis = data.Enum<SpeedAxis>("Axis");
        Comparison = data.Enum<ComparisonType>("Comparison");
        Action = data.Enum<TriggerAction>("Action");
        FlagName = data.String("FlagName");
        SetSpeed = new(data.Float("SetX"), data.Float("SetY"));
        lineDir = new Vector2((float) Math.Cos(Angle), (float) Math.Sin(Angle));
        lineNorm = new Vector2((float) -Math.Sin(Angle), (float) Math.Cos(Angle));
    }

    public void OnTrigger(Player player)
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

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Line(Position - lineDir * Size / 2, Position + lineDir * Size / 2, Color.Cyan);
    }

    private bool CollideCheck(Player player)
    {
        var prevPos = player.PreviousPosition;
        var delta = player.Position - prevPos;
        var d1 = Vector2.Dot(prevPos - Position, lineNorm);
        var d2 = Vector2.Dot(prevPos + delta - Position, lineNorm);
        if (d1 * d2 > 0)
            return false;
        var t = d1 / (d1 - d2);
        var crossPoint = prevPos + t * delta;
        var proj = Vector2.Dot(crossPoint - Position, lineDir);
        return Math.Abs(proj) <= Size;
    }
    
    public static void LoadHooks()
    {
        On.Celeste.Player.Update += UpdateHook;
    }

    public static void UnloadHooks()
    {
        On.Celeste.Player.Update -= UpdateHook;
    }

    private static void UpdateHook(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        foreach (SpeedcheckGate gate in self.level.Tracker.GetEntities<SpeedcheckGate>())
            if (gate.CollideCheck(self))
                gate.OnTrigger(self);
    }
}
