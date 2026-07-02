using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/DebugViewTrigger")]
[Tracked(false)]
public class DebugViewTrigger(EntityData e, Vector2 offset) : Trigger(e, offset)
{
    internal static bool ForceRenderDebug;

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        ForceRenderDebug = true;
    }
    public override void OnLeave(Player player) {
        base.OnLeave(player);
        ForceRenderDebug = false;
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStartLevel;
        if (!HookUtils.TryDisableInlining(typeof(GameplayRenderer).GetMethod("Render", [typeof(Scene)])))
            throw new Utils.HookException("Failed to disable inlining.");
        IL.Celeste.GameplayRenderer.Render += RenderHook;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStartLevel;
        IL.Celeste.GameplayRenderer.Render -= RenderHook;
    }

    private static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
        ForceRenderDebug = false;
        orig(self);
    }

    private static void OnLevelLoaderStartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
        ForceRenderDebug = false;
        orig(self);
    }

    private static void RenderHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Monocle.Commands>("Open")))
            throw new InvalidOperationException("Hitbox view trigger failed to match IL code for the Render hook.");
        static bool Delegate() { return ForceRenderDebug; }
        cur.EmitDelegate(Delegate);
        cur.EmitOr();
    }
}
