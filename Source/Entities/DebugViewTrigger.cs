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
public class DebugViewTrigger(EntityData e, Vector2 offset) : Trigger(e, offset) {
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        ScugHelperModule.Instance.ForceRenderDebug = true;
    }
    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        ScugHelperModule.Instance.ForceRenderDebug = false;
    }
    [OnLoad]
    public static void LoadHooks()
    {
        IL.Celeste.GameplayRenderer.Render += RenderHook;
    }
    [OnUnload]
    public static void UnloadHooks() {
        IL.Celeste.GameplayRenderer.Render -= RenderHook;
    }

    private static void RenderHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Monocle.Commands>("Open")))
            throw new InvalidOperationException("Hitbox view trigger failed to match IL code for the Render hook.");
        static bool Delegate() { return ScugHelperModule.Instance.ForceRenderDebug; }
        cur.EmitDelegate(Delegate);
        cur.EmitOr();
    }
}
