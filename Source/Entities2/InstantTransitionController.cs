using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using FMOD.Studio;
using System;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.Cil;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/InstantTransitionController")]
public class InstantTransitionController() : Entity() {
    private static Type? orig_TransitionRoutineEnumeratorType;
    private static ILHook? ILLevelorig_TransitionRoutineHook;

    [OnLoad]
    public static void LoadHooks() {
        var stTarget = typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance)!.GetStateMachineTarget()!;
        orig_TransitionRoutineEnumeratorType = stTarget.DeclaringType;
        ILLevelorig_TransitionRoutineHook = new(stTarget, ILLevelorig_TransitionRoutine);
    }

    [OnUnload]
    public static void UnloadHooks() {
        ILLevelorig_TransitionRoutineHook?.Dispose();
    }
    
    private static void ILLevelorig_TransitionRoutine(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After,
            static match => match.MatchLdarg0(),
            static match => match.MatchLdcR4(0.0f)
        )) throw new Exception("Failed to hook for instant level transitions.");
        static float SwapForInstant(float orig, Level level, Vector2 direction) {
            bool fromController = level.Tracker.GetEntity<InstantTransitionController>() is not null;
            if (fromController || ScugHelperModule.Settings.InstantRoomTransitions) {
                float newLighting = level.DarkRoom ? level.Session.DarkRoomAlpha : (level.BaseLightingAlpha + level.Session.LightingAlphaAdd);
                if (level.Lighting.Alpha < newLighting)
                    Audio.Play("event:/game/05_mirror_temple/room_lightlevel_down");
                else if (level.Lighting.Alpha > newLighting)
                    Audio.Play("event:/game/05_mirror_temple/room_lightlevel_up");
                level.Lighting.Alpha = newLighting;
                level.NextTransitionDuration = 0f;
                if (!fromController) {
                    long transitionTicks = TimeSpan.FromSeconds(40 * 0.017).Ticks;
                    SaveData.Instance.AddTime(level.Session.Area, transitionTicks);
                    if (!level.Completed && level.TimerStarted) level.Session.Time += transitionTicks;
                }
                Vector2 dirPad = direction * (direction == Vector2.UnitY ? 12f : 4f);
                Player player = level.Tracker.GetEntity<Player>();
                Vector2 playerTo = player.Position;
                while (direction.X != 0f && playerTo.Y >= level.Bounds.Bottom)
                    playerTo.Y -= 1f;
                for (; !level.IsInBounds(playerTo, dirPad); playerTo += direction) {}
                Vector2 cameraTo = level.GetFullCameraTargetAt(player, playerTo);
                level.Camera.Position = cameraTo;
                
                int iters = 0;
                while (!player.TransitionTo(playerTo, direction))
                    if (iters++ > 10000)
                        throw new Exception("What the fuck!?");
                        
                return 1.0f;
            }
            return orig;
        }
        cur.EmitLdloc1();
        cur.EmitLdarg0();
        cur.EmitLdfld(orig_TransitionRoutineEnumeratorType!.GetField("direction", BindingFlags.Public | BindingFlags.Instance)!);
        cur.EmitDelegate(SwapForInstant);
    }
}
