using System;
using Microsoft.Xna.Framework;
using Monocle;

public static class WobblyHelper {
    public static void RenderOutline(Rectangle bounds, float elapsed, float waveSpeed, float amplitude, Color color)
    {
        for (int i = 0; i <= bounds.Height; i++)
        {
            float factor = Math.Min(1.0f, Math.Min(i / 16.0f, (bounds.Height - i) / 16.0f)) * amplitude;
            int y = bounds.Top + i;
            float offset = (float)Math.Ceiling((Math.Sin((i / 4.0f) + elapsed * waveSpeed) / 2.0f + 1.0f) * factor);
            Draw.Point(new(bounds.Left - offset, y), color);
            Draw.Point(new(bounds.Right + offset, y), color);
        }
        for (int i = 1; i <= bounds.Width - 1; i++)
        {
            float factor = Math.Min(1.0f, Math.Min(i / 16.0f, (bounds.Width - i) / 16.0f)) * amplitude;
            int x = bounds.Left + i;
            float offset = (float)Math.Ceiling((Math.Sin((i / 4.0f) + elapsed * waveSpeed) / 2.0f + 1.0f) * factor);
            Draw.Point(new(x, bounds.Top - offset), color);
            Draw.Point(new(x, bounds.Bottom + offset), color);
        }
    }
    
    public static void RenderFill(Rectangle bounds, float elapsed, float waveSpeed, float amplitude, Color color) {
        Draw.Rect(bounds, color);
        for (int i = 0; i <= bounds.Height; i++)
        {
            float factor = Math.Min(1.0f, Math.Min(i / 16.0f, (bounds.Height - i) / 16.0f)) * amplitude;
            int y = bounds.Top + i;
            float offset = (float)Math.Ceiling((Math.Sin((i / 4.0f) + elapsed * waveSpeed) / 2.0f + 1.0f) * factor);
            Draw.Line(new(bounds.Left, y), new(bounds.Left - offset, y), color);
            Draw.Line(new(bounds.Right, y), new(bounds.Right + offset + 1, y), color);
        }
        for (int i = 0; i <= bounds.Width; i++)
        {
            float factor = Math.Min(1.0f, Math.Min(i / 16.0f, (bounds.Width - i) / 16.0f)) * amplitude;
            int x = bounds.Left + i;
            float offset = (float)Math.Ceiling((Math.Sin((i / 4.0f) + elapsed * waveSpeed) / 2.0f + 1.0f) * factor);
            Draw.Line(new(x, bounds.Top), new(x, bounds.Top - offset), color);
            Draw.Line(new(x, bounds.Bottom), new(x, bounds.Bottom + offset + 1), color);
        }
    }
}