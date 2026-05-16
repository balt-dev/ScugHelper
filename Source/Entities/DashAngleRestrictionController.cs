using System;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;
using System.Linq;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/DashAngleRestrictionTrigger")]
public class DashAngleRestrictionTrigger : Trigger
{
    public readonly Vector2[] Angles;
    public readonly bool FlagState;
    public readonly string Flag;

    public DashAngleRestrictionTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        Angles = data.String("Angles")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(str => float.Parse(str, System.Globalization.NumberStyles.Float))
            .Select(angle => Calc.AngleToVector(angle.ToRad(), 1))
            .ToArray();
        if (Angles.Length == 0) throw new FormatException($"Must have at least one angle for dash angle restriction trigger. (Room: {data.Level.Name}, ID: {data.ID})");
        FlagState = data.Bool("FlagState", true);
        Flag = data.String("Flag");
        if (data.Bool("CoverRoom", false))
        {
            Position = data.Level.Position;
            Collider = new Hitbox(data.Level.Bounds.Width + 256, data.Level.Bounds.Height + 256, -128, -128);
        }
    }

    DashRestrictorComponent? comp;

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (Flag is null || player.level.Session.GetFlag(Flag) == FlagState)
            player.Add(comp = new DashRestrictorComponent(Angles));
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        if (Flag is null) return;
        if (comp is not null && player.level.Session.GetFlag(Flag) != FlagState)
        {
            player.Remove(comp);
            comp = null;
        }
        else if (comp is null && player.level.Session.GetFlag(Flag) == FlagState)
        {
            player.Add(comp = new DashRestrictorComponent(Angles));
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (comp is null) return;
        player.Remove(comp);
        comp = null;
    }

    [OnLoad]
    internal static void LoadHooks()
    {
        On.Celeste.Player.CorrectDashPrecision += OnCorrectDashPrecision;
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
    }

    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Celeste.Player.CorrectDashPrecision -= OnCorrectDashPrecision;
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
    }

    private static Vector2 OnCorrectDashPrecision(On.Celeste.Player.orig_CorrectDashPrecision orig, Player self, Vector2 dir)
    {
        var res = orig(self, dir);
        foreach (DashRestrictorComponent component in self.Components.GetAll<DashRestrictorComponent>())
        {
            Vector2 closestVector = -res;
            float closestDot = -1;
            foreach (Vector2 angle in component.Angles)
            {
                var dot = Vector2.Dot(angle, res);
                if (dot > closestDot)
                {
                    closestVector = res;
                    closestDot = dot;
                }
            }
            res = closestVector;
        }
        return res;
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        player.Components.RemoveAll<DashRestrictorComponent>();
    }

    private class DashRestrictorComponent(Vector2[] angles) : Component(false, false)
    {
        internal Vector2[] Angles = angles;
    }
}
