using System;
using System.Reflection;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.ScugHelper;

internal static class InstantRoomTransitions
{
    private static ILHook? ILLevelorig_Transition;

    [OnLoad]
    public static void LoadHooks() {
        ILLevelorig_Transition = new(typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance)!.GetStateMachineTarget()!, ILLevelTransition);
    }

    [OnUnload]
    public static void UnloadHooks() {
        ILLevelorig_Transition?.Dispose();
    }
    
    private static void ILLevelTransition(ILContext il)
    {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After,
            static match => match.MatchLdarg0(),
            static match => match.MatchLdcR4(0.0f)
        )) throw new Exception("Failed to hook for instant level transitions.");
        static float SwapForInstant(float orig, Level level) {
            if (ScugHelperModule.Settings.InstantRoomTransitions) {
                long transitionTicks = TimeSpan.FromSeconds(40 * 0.017).Ticks;
                SaveData.Instance.AddTime(level.Session.Area, transitionTicks);
                if (!level.Completed && level.TimerStarted) level.Session.Time += transitionTicks;
                return 1.0f;
            }
            return orig;
        }
        cur.EmitLdloc1();
        cur.EmitDelegate(SwapForInstant);
    }
}
