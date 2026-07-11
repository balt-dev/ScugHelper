using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Celeste.Mod.Helpers;
using Celeste.Mod.Registry;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.UI;
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

    public static float Mod(this float self, float dividend) => ((self % dividend) + dividend) % dividend;

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

    public static T Clone<T>(this T self) {
        using var stream = new MemoryStream();
        var serializer = new DataContractSerializer(typeof(T));
        serializer.WriteObject(stream, self);
        stream.Position = 0;
        return (T) serializer.ReadObject(stream)!;
    }

    public static Color Mul (this Color self, Color other) => new(self.ToVector4() * other.ToVector4());

    internal static MethodInfo GetMethodInfo(LambdaExpression expr)
        => expr.Body is MethodCallExpression outerExpr
                    ? outerExpr.Method
                    : throw new ArgumentException("UninlineMethod be given a lambda in the form of '(...) => f(...)'.");
    internal static void UninlineMethod(LambdaExpression expr) => UninlineMethod(GetMethodInfo(expr));
    internal static void UninlineMethod(MethodInfo info) {
        if (!HookUtils.TryDisableInlining(info))
            throw new HookException($"Failed to uniniline method {info}.");
    }

    internal static void Add<K, V>(this Dictionary<K, V> dict, KeyValuePair<K, V> kvp) where K: notnull => dict.Add(kvp.Key, kvp.Value);
    internal static void Add<V>(this Stack<V> stack, V value) => stack.Push(value);
    internal static V? GetValueOrNull<K, V>(this Dictionary<K, V> dict, K key) where K: notnull where V: struct
        => dict.TryGetValue(key, out var val) ? val : null;
    internal static V? GetNullableValue<K, V>(this Dictionary<K, V> dict, K key) where K: notnull where V: class
        => dict.TryGetValue(key, out var val) ? val : null;

    static readonly Dictionary<string, Type?> TypeCache = [];

    internal static Type? GetTypeOfEntity(EntityData data) {
        if (TypeCache.TryGetValue(data.Name, out var res)) return res;
        var type = EntityRegistry.GetKnownTypesFromSid(data.Name).AsEnumerable().FirstOrDefault((Type?)null);
        if (type is not Type ty)
            Logger.Warn(nameof(ScugHelper), $"SID {data.Name} of entity with ID {data.ID} does not correspond to any known types.");
        TypeCache[data.Name] = type;
        return type;
    }

    static readonly Dictionary<Type, IReadOnlySet<string>> NameCache = [
        new(typeof(Player), new HashSet<string>(["player"])),
        new(typeof(SolidTiles), new HashSet<string>(["fg"])),
        new(typeof(BackgroundTiles), new HashSet<string>(["bg"]))
    ];
    internal static IReadOnlySet<string> GetNamesOfEntity(Entity entity) => GetNamesOfEntity(entity.GetType());
    internal static IReadOnlySet<string> GetNamesOfEntity(Type type) {
        if (NameCache.TryGetValue(type, out var res)) return res;
        var sids = EntityRegistry.GetKnownSidsFromType(type);
        NameCache[type] = sids;
        return sids;
    }

    public static Vector3 ToHsv (this Color self) {
        Vector3 rgb = self.ToVector3();
        double h = 0;
        double v = Math.Max(Math.Max(rgb.X, rgb.Y), rgb.Z);

    	double min = Math.Min(Math.Min(rgb.X, rgb.Y), rgb.Z);
    	double delta = v - min;

    	double s = v == 0.0 ? 0 : delta / v;

    	if (s == 0) h = 0.0;
    	else if (rgb.X == v) h = (rgb.Y - rgb.Z) / delta;
  		else if (rgb.Y == v) h = 2 + (rgb.Z - rgb.X) / delta;
  		else if (rgb.Z == v) h = 4 + (rgb.X - rgb.Y) / delta;
        h /= 6;

        return new((float)h, (float)s, (float)v);
    }

    public static bool RecoverFromInvalidPosition(this Actor self) {
        if (!(float.IsFinite(self.movementCounter.X) && float.IsFinite(self.movementCounter.Y) && float.IsFinite(self.X) && float.IsFinite(self.Y))) {
            self.movementCounter = self.Position = Vector2.Zero;
            throw new InvalidOperationException("Actor position is non-finite. Bailing out.");
        }
        return true;
    }

    internal static string FormatNumber(float s) => !float.IsFinite(s) ? $"{s}" : MathF.Abs(s) > 1e10 ? $"{s:E9}" : $"{s:F0}";
}


