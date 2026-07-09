using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Runtime.InteropServices;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/SessionVariableTrigger")]
public class SessionVariableTrigger : Trigger {
    internal enum VariableType { Flag, Counter, Slider }
    [StructLayout(LayoutKind.Explicit)]
    internal struct VariableValue {
        [FieldOffset(0)] internal bool Flag;
        [FieldOffset(0)] internal int Counter;
        [FieldOffset(0)] internal float Slider;
    }

    internal readonly string Name;
    internal readonly bool ResetOnLeave;
    internal readonly bool CoverRoom;
    internal readonly VariableType Type;
    internal readonly VariableValue Value;
    internal VariableValue? OldValue;

    public SessionVariableTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Name = data.String("Name") ?? throw new ArgumentException("Must supply a variable name.");
        Type = data.Enum<VariableType>("Type");
        Value = Type switch {
            VariableType.Flag => new() { Flag = bool.Parse(data.String("Value")) },
            VariableType.Counter => new() { Counter = int.Parse(data.String("Value")) },
            VariableType.Slider => new() { Slider = float.Parse(data.String("Value")) },
        };
        CoverRoom = data.Bool("CoverRoom", false);
        ResetOnLeave = data.Bool("ResetOnLeave", false);
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        if (scene is not Level level) return;
        if (CoverRoom) {
            Collidable = false;
            ArmOldValue(level.Session);
            UpdateValue(level.Session);
        }
    }

    public override void Update() {
        base.Update();
        if (Scene is not Level level) return;
        if (CoverRoom) UpdateValue(level.Session);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (CoverRoom) return;
        ArmOldValue(player.level.Session);
        UpdateValue(player.level.Session);
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        if (CoverRoom) return;
        if (player.level?.Session is Session session)
            UpdateValue(session);
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);
        if (CoverRoom) return;
        if (player.level?.Session is Session session)
            ResetValue(session);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (scene is Level level && level.Session is not null) {
            ResetValue(level.Session);
        }
    }

    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        if (scene is Level level && level.Session is not null)
            ResetValue(level.Session);
    }

    private void ArmOldValue(Session session) {
        if (ResetOnLeave)
            OldValue = Type switch {
                VariableType.Flag => new() { Flag = session.GetFlag(Name) },
                VariableType.Counter => new() { Counter = session.GetCounter(Name) },
                VariableType.Slider => new() { Slider = session.GetSlider(Name) },
            };
    }

    private void UpdateValue(Session session) {
        switch (Type) {
            case VariableType.Flag: {
                session.SetFlag(Name, Value.Flag);
                break;
            }
            case VariableType.Counter: {
                session.SetCounter(Name, Value.Counter);
                break;
            }
            case VariableType.Slider: {
                session.SetSlider(Name, Value.Slider);
                break;
            }
        }
    }

    private void ResetValue(Session session) {
        if (OldValue is not {} oldValue) return;
        OldValue = null;
        switch (Type) {
            case VariableType.Flag: {
                session.SetFlag(Name, oldValue.Flag);
                break;
            }
            case VariableType.Counter: {
                session.SetCounter(Name, oldValue.Counter);
                break;
            }
            case VariableType.Slider: {
                session.SetSlider(Name, oldValue.Slider);
                break;
            }
        }
    }
}
