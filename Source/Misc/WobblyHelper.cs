using System;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.ScugHelper;

public static class WobblyHelper {
    private static float CalculateOffset(int i, float factor, float waveSpeed, float elapsed) {
        double waveA = Math.Sin((i / 4.0) + elapsed * waveSpeed) / 2.0 + 0.5;
        double waveB = Math.Sin(((9 - i) / 13.17) + elapsed * 0.32 * waveSpeed) / 2.0 + 0.5;
        double waveC = Math.Sin((i / 22.03) + elapsed * 0.13 * waveSpeed) / 2.0 + 0.5;
        return (float)Math.Ceiling((waveA + waveB + waveC) / 3f * 1.3 * factor);
    }

    public static void RenderOutline(
        Rectangle bounds, float elapsed, float waveSpeed, float amplitude, Color color
    ) {
        amplitude = MathF.Max(amplitude, 0.01f);
        for (int i = 1; i <= bounds.Height - 2; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Height - i) / 8.0f)) * amplitude;
            int y = bounds.Top + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Point(new(bounds.Left + 1 - offset, y), color);
            Draw.Point(new(bounds.Right - 2 + offset, y), color);
        }
        for (int i = 1; i <= bounds.Width - 2; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Width - i) / 8.0f)) * amplitude;
            int x = bounds.Left + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Point(new(x, bounds.Top + 1 - offset), color);
            Draw.Point(new(x, bounds.Bottom - 2 + offset), color);
        }
    }

    public static void RenderFill(
        Rectangle actualBounds, float elapsed,
        float waveSpeed, float amplitude,
        Color fieldColor
    ) {
        amplitude = MathF.Max(amplitude, 0.01f);
        Rectangle bounds = new(actualBounds.X + 1, actualBounds.Y + 1, actualBounds.Width - 2, actualBounds.Height - 2);
        
        Draw.Rect(bounds, fieldColor);
        for (int i = 2; i < bounds.Height - 1; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Height - i) / 8.0f)) * amplitude;
            int y = bounds.Top + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Line(new(bounds.Left, y), new(bounds.Left - offset, y), fieldColor);
            Draw.Line(new(bounds.Right, y), new(bounds.Right + offset, y), fieldColor);
        }
        for (int i = 2; i < bounds.Width - 1; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Width - i) / 8.0f)) * amplitude;
            int x = bounds.Left + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Line(new(x, bounds.Top), new(x, bounds.Top - offset), fieldColor);
            Draw.Line(new(x, bounds.Bottom), new(x, bounds.Bottom + offset), fieldColor);
        }        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
    
    public static void RenderFill(
        Camera camera,
        Rectangle actualBounds, float elapsed,
        float waveSpeed, float amplitude,
        Color fieldColor, Color particleColor
    ) {
        amplitude = MathF.Max(amplitude, 0.01f);
        Rectangle bounds = new(actualBounds.X + 1, actualBounds.Y + 1, actualBounds.Width - 2, actualBounds.Height - 2);

        var renderTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();

        GameplayRenderer.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
        Draw.Rect(bounds, Color.White);
        for (int i = 2; i < bounds.Height - 1; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Height - i) / 8.0f)) * amplitude;
            int y = bounds.Top + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Line(new(bounds.Left, y), new(bounds.Left - offset, y), Color.White);
            Draw.Line(new(bounds.Right, y), new(bounds.Right + offset, y), Color.White);
        }
        for (int i = 2; i < bounds.Width - 1; i++) {
            float factor = Math.Min(1.0f, Math.Min(i / 8.0f, (bounds.Width - i) / 8.0f)) * amplitude;
            int x = bounds.Left + i;
            float offset = CalculateOffset(i, factor, waveSpeed, elapsed);
            Draw.Line(new(x, bounds.Top), new(x, bounds.Top - offset), Color.White);
            Draw.Line(new(x, bounds.Bottom), new(x, bounds.Bottom + offset), Color.White);
        }
        Draw.SpriteBatch.End();
        Engine.Graphics.GraphicsDevice.SetRenderTargets(renderTargets);
        ScugHelperModule.SeekerBarrierFX?.Parameters["ParticleColor"].SetValue(particleColor.ToVector4());
        ScugHelperModule.SeekerBarrierFX?.Parameters["CameraPosition"].SetValue(camera.Position);
        ScugHelperModule.SeekerBarrierFX?.Parameters["TexelSize"].SetValue(new Vector2(1f / GameplayBuffers.TempA.Width, 1f / GameplayBuffers.TempA.Height));
        ScugHelperModule.SeekerBarrierFX?.Parameters["ActiveTime"].SetValue(elapsed);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.SeekerBarrierFX, camera.Matrix);
        Draw.SpriteBatch.Draw(GameplayBuffers.TempA, camera.Position, fieldColor);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
}
