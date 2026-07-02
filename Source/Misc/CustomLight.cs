using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.ScugHelper;

[Tracked]
internal class CustomLight(Action onRenderLight) : Component(false, false)
{
    internal Action OnRenderLight = onRenderLight;

    [OnLoad] internal static void LoadHooks() => IL.Celeste.LightingRenderer.BeforeRender += ILLightingRendererBeforeRender;
    [OnUnload] internal static void UnloadHooks() => IL.Celeste.LightingRenderer.BeforeRender -= ILLightingRendererBeforeRender;

    private static void RenderCustomLights(LightingRenderer self, Scene scene)
    {
        List<Component> comps = scene.Tracker.GetComponents<CustomLight>();
        if (comps.Count == 0) return;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (scene as Level)!.Camera.Matrix);
        foreach (CustomLight light in comps)
            light.OnRenderLight();
        Draw.SpriteBatch.End();
    }

    private static void ILLightingRendererBeforeRender(ILContext il)
    {
        ILCursor cur = new(il);
        // Go to end of function
        while (cur.TryGotoNext(MoveType.After, static match => match.MatchRet())) { }

        if (!cur.TryGotoPrev(
            MoveType.AfterLabel, static match => match.MatchCallOrCallvirt(typeof(GaussianBlur), nameof(GaussianBlur.Blur))
        )) throw new Utils.HookException("Failed to hook LightingRenderer for CustomLight component!");

        cur.EmitLdarg0();
        cur.EmitLdarg1();
        cur.EmitDelegate(RenderCustomLights);
    }
}