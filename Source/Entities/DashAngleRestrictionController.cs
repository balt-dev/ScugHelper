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

    public DashAngleRestrictionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Angles = data.String("Angles")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(str => float.Parse(str, System.Globalization.NumberStyles.Float))
            .Select(angle => Calc.AngleToVector(angle.ToRad(), 1))
            .ToArray();
        Logger.Log(nameof(ScugHelperModule), $"New dash angle restriction trigger: {string.Join(',', Angles)}");
        if (Angles.Length == 0) throw new FormatException($"Must have at least one angle for dash angle restriction trigger. (Room: {data.Level.Name}, ID: {data.ID})");
        FlagState = data.Bool("FlagState", true);
        Flag = data.String("Flag");
        if (data.Bool("CoverRoom", false)) {
            Position = data.Level.Position;
            Collider = new Hitbox(data.Level.Bounds.Width + 256, data.Level.Bounds.Height + 256, -128, -128);
        }
    }

    DashRestrictorComponent? comp;

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (Flag is null || player.level.Session.GetFlag(Flag) == FlagState)
            player.Add(comp = new DashRestrictorComponent(Angles));
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        if (Flag is null) return;
        if (comp is not null && player.level.Session.GetFlag(Flag) != FlagState) {
            player.Remove(comp);
            comp = null;
        } else if (comp is null && player.level.Session.GetFlag(Flag) == FlagState) {
            player.Add(comp = new DashRestrictorComponent(Angles));
        }
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);
        if (comp is null) return;
        player.Remove(comp);
        comp = null;
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Player.CorrectDashPrecision += OnCorrectDashPrecision;
        IL.Celeste.PlayerDashAssist.Update += ILDashAssistUpdate;
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Player.CorrectDashPrecision -= OnCorrectDashPrecision;
        IL.Celeste.PlayerDashAssist.Update -= ILDashAssistUpdate;
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
    }

    private static void ILDashAssistUpdate(ILContext il) {
        ILCursor cur = new(il);
        
        if (!cur.TryGotoNext(MoveType.After, 
            static match => match.MatchCall(typeof(Input), nameof(Input.GetAimVector))
        )) throw new Exception("Failed to match IL for dash angle restriction trigger.");

        cur.EmitLdloc0();
        static Vector2 DashPrecision(Vector2 dir, Player self) => self.CorrectDashPrecision(dir);
        cur.EmitDelegate(DashPrecision);
    }

    private static Vector2 OnCorrectDashPrecision(On.Celeste.Player.orig_CorrectDashPrecision orig, Player self, Vector2 dir) {
        foreach (DashRestrictorComponent component in self.Components.GetAll<DashRestrictorComponent>()) {
            if (Math.Abs(dir.X) < 0.001) {
                dir.X += (int)self.Facing * 0.001f;
                dir.Normalize();
            }
            Logger.Log(nameof(ScugHelperModule), $"Angles to choose from: {string.Join(',', component.Angles)}");
            Vector2 closestVector = Vector2.Zero;
            float closestDot = float.NegativeInfinity;
            foreach (Vector2 angle in component.Angles) {
                var dot = Vector2.Dot(angle, dir);
                if (dot >= closestDot) {
                    closestVector = angle;
                    closestDot = dot;
                }
            }
            dir = closestVector;
            Logger.Log(nameof(ScugHelperModule), $"Closest vector: {dir}");
        }
        return orig(self, dir);
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        player.Components.RemoveAll<DashRestrictorComponent>();
    }

    private class DashRestrictorComponent : Component
    {
        internal Vector2[] Angles;

        public DashRestrictorComponent(Vector2[] angles) : base(false, false) {
            Angles = angles;
            Logger.Log(nameof(ScugHelperModule), $"New dash restrictor component: {string.Join(',', Angles)}");
        }
    }
}
