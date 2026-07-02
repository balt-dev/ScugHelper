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

internal static class PixelDistortion {
    [OnLoad] internal static void LoadHooks() => IL.Celeste.Level.Render += ILLevelRender;
    [OnUnload] internal static void UnloadHooks() => IL.Celeste.Level.Render -= ILLevelRender;
    
    internal static VirtualRenderTarget? DistortionBuffer;
    internal static VirtualRenderTarget? ScratchBuffer;
    
    private static void ILLevelRender(ILContext il) {
        ILCursor cur = new(il);

        if (!cur.TryGotoNext(
            MoveType.AfterLabel,
            static match => match.MatchCallOrCallvirt(typeof(Glitch), nameof(Glitch.Apply))
        )) throw new Utils.HookException("Failed to hook Level.Render for HallOfMirrors styleground!");

        cur.EmitLdarg0();
        cur.EmitDelegate(RenderDistortion);
    }
    
    private static void RenderDistortion(Level level) {
        if (level.Tracker.CountComponents<PixelDistortionRenderer>() == 0) return;
        
        if (
            DistortionBuffer is null ||
            DistortionBuffer.Width != GameplayBuffers.Gameplay.Width ||
            DistortionBuffer.Height != GameplayBuffers.Gameplay.Height
        ) {
            DistortionBuffer?.Dispose();
            DistortionBuffer = VirtualContent.CreateRenderTarget($"ScugDistortionBuffer", GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);
        }
        if (
            ScratchBuffer is null ||
            ScratchBuffer.Width != GameplayBuffers.Gameplay.Width ||
            ScratchBuffer.Height != GameplayBuffers.Gameplay.Height
        ) {
            ScratchBuffer?.Dispose();
            ScratchBuffer = VirtualContent.CreateRenderTarget($"ScugDistortionScratchBuffer", GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);
        }

        var oldRT = Engine.Graphics.GraphicsDevice.GetRenderTargets();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(DistortionBuffer);
        Engine.Graphics.GraphicsDevice.Clear(new Color(0x80, 0x80, 0xFF, 0xFF));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
        foreach (PixelDistortionRenderer renderer in level.Tracker.GetComponentsTrackIfNeeded<PixelDistortionRenderer>())
            renderer.OnRender();
        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(ScratchBuffer);
        Engine.Graphics.GraphicsDevice.Textures[1] = DistortionBuffer;
        
        ScugHelperModule.PixelDistortionFX?.Parameters["BufferSize"]
            .SetValue(new Vector2(ScratchBuffer.Width, ScratchBuffer.Height));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ScugHelperModule.PixelDistortionFX, Matrix.Identity);
        Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
        
        Engine.Graphics.GraphicsDevice.SetRenderTargets(oldRT);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(ScratchBuffer, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
        foreach (PixelDistortionRenderer renderer in level.Tracker.GetComponentsTrackIfNeeded<PixelDistortionRenderer>())
            renderer.OnRenderOverlay?.Invoke();
        Draw.SpriteBatch.End();
    }
}

[Tracked]
internal class PixelDistortionRenderer(Action render, Action? overlayRender = null) : Component(false, false) {
    internal readonly Action OnRender = render;
    internal readonly Action? OnRenderOverlay = overlayRender;
}