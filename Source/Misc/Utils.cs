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

internal static class Utils
{
    internal static ulong HashPosition(int a, int b, int c = 0) {
        ulong seed = (uint)a;
        seed = (seed << 32) | (uint)b;
        seed ^= (ulong) c << 16 ;

        seed += 0x9e3779b97f4a7c15UL;
        ulong z = seed;
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
        z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
        z = z ^ (z >> 31);
        return z;
    }

    public static class Perlin {
        // See: https://adrianb.io/2014/08/09/perlinnoise.html

        private static double Fade(double t) {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        public static double Grad(int hash, double x, double y) {
            int h = hash & 7;
            double u = h < 4 ? x : y;
            double v = h < 4 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public static double PerlinNoise(double x, double y, int octaves, double persistence, int seed) {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;
            for(int i=0; i<octaves; i++) {
                total += RawPerlinNoise(x * frequency, y * frequency, seed) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }

            return total/maxValue;
        }

        public static double RawPerlinNoise(double x, double y, int seed) {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            double xf = x - xi;
            double yf = y - yi;
            double u = Fade(xf);
            double v = Fade(yf);

            int aa, ab, ba, bb;
            aa = (int) HashPosition((int) Math.Floor(x    ), (int) Math.Floor(y    ), seed);
            ab = (int) HashPosition((int) Math.Floor(x    ), (int) Math.Floor(y + 1), seed);
            ba = (int) HashPosition((int) Math.Floor(x + 1), (int) Math.Floor(y    ), seed);
            bb = (int) HashPosition((int) Math.Floor(x + 1), (int) Math.Floor(y + 1), seed);

            double x1, x2;
            x1 = double.Lerp(Grad(aa, xf, yf), Grad (ba, xf-1, yf), u);
            x2 = double.Lerp(Grad(ab, xf, yf-1), Grad(bb, xf-1, yf-1), u);

            return (double.Lerp(x1, x2, v) + 1) / 2;
        }
    }

    public static readonly BlendState AlphaMaskBlendState = new() {
        Name = "BlendState.ScugHelper.AlphaMask",
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.DestinationColor,
        AlphaDestinationBlend = Blend.Zero,
        AlphaBlendFunction = BlendFunction.Add,
    };

    public static readonly BlendState AdditiveMaskAlphaBlendState = new() {
        Name = "BlendState.ScugHelper.AdditiveMaskAlpha",
        ColorSourceBlend = Blend.DestinationAlpha,
        ColorDestinationBlend = Blend.DestinationAlpha,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add,
    };

    public static readonly BlendState AdditiveKeepAlphaBlendState = new() {
        Name = "BlendState.ScugHelper.AdditiveKeepAlpha",
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add,
    };

    public static Vector2 Rounded(this Vector2 self) => new(MathF.Round(self.X), MathF.Round(self.Y));

    public static Rectangle Bounds(this Camera self) => new(
        (int) self.Left, (int) self.Top,
        (int) (self.Right - self.Left), (int) (self.Bottom - self.Top)
    );

    public static bool Contains(this Rectangle self, Vector2 position) =>
        self.Left > position.X &&
        self.Right < position.X &&
        self.Top > position.Y &&
        self.Bottom < position.Y;
    
    public static Rectangle? Intersection(this Rectangle self, Rectangle other) {
        var res = new Rectangle(
            (int) MathF.Max(self.Left, other.Left),
            (int) MathF.Max(self.Top, other.Top),
            (int) MathF.Min(self.Width, other.Right - self.Left),
            (int) MathF.Min(self.Height, other.Bottom - self.Top)
        );
        return res.Width <= 0 || res.Height <= 0 ? null : res;
    }

    public static Rectangle Grow(this Rectangle self, int margin)
        => new(self.Left - margin, self.Top - margin, self.Width + margin * 2, self.Height + margin * 2);

    public static int BufferWidth = 320 * 2;
    public static int BufferHeight = 184 * 2;

    internal class HookException(string? message) : Exception(message) {}

    [Tracked]
    internal class CustomLight(Action onRenderLight) : Component(false, false) { internal Action OnRenderLight = onRenderLight; }

    [OnLoad]
    internal static void LoadHooks() => IL.Celeste.LightingRenderer.BeforeRender += ILLightingRendererBeforeRender;
    [OnUnload]
    internal static void UnloadHooks() => IL.Celeste.LightingRenderer.BeforeRender -= ILLightingRendererBeforeRender;

    private static void RenderCustomLights(LightingRenderer self, Scene scene) {
        List<Component> comps = scene.Tracker.GetComponents<CustomLight>();
        if (comps.Count == 0) return;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (scene as Level)!.Camera.Matrix);
        foreach (CustomLight light in comps)
            light.OnRenderLight();
        Draw.SpriteBatch.End();
    }

    private static void ILLightingRendererBeforeRender(ILContext il) {
        ILCursor cur = new(il);
        // Go to end of function
        while (cur.TryGotoNext(MoveType.After, static match => match.MatchRet())) { }

        if (!cur.TryGotoPrev(
            MoveType.AfterLabel, static match => match.MatchCallOrCallvirt(typeof(GaussianBlur), nameof(GaussianBlur.Blur))
        )) throw new HookException("Failed to hook LightingRenderer for CustomLight component!");

        cur.EmitLdarg0();
        cur.EmitLdarg1();
        cur.EmitDelegate(RenderCustomLights);
    }
    
    public static T Clone<T>(this T self) {
        using var stream = new MemoryStream();
        var serializer = new DataContractSerializer(typeof(T));
        serializer.WriteObject(stream, self);
        stream.Position = 0;
        return (T) serializer.ReadObject(stream)!;
    }
    
    public static Color Mul (this Color self, Color other) => new(self.ToVector4() * other.ToVector4());
}
