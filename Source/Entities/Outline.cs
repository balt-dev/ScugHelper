using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework.Graphics;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/Outline")]
public class Outline : Entity
{

    private readonly Color Color;
    private readonly float InnerOpacity;
    private readonly int LineSize;
    private readonly int SpaceSize;
    private readonly int CornerSize;
    private readonly int CornerSpace;
    private readonly int InnerMargin;

    public Outline(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collidable = false;
        Collider = new Hitbox(data.Width, data.Height);
        Depth = data.Int("Depth", 10);
        Color = data.HexColor("Color", Color.White);
        InnerOpacity = data.Float("InnerOpacity", 0.25f);
        LineSize = data.Int("LineSize", 2);
        SpaceSize = data.Int("SpaceSize", 1);
        CornerSize = data.Int("CornerSize", 2);
        CornerSpace = data.Int("CornerSpace", 1);
        InnerMargin = data.Int("InnerMargin", 4);
    }

    public override void Render() {
        base.Render();
        Draw.Rect(Left + InnerMargin, Top + InnerMargin, Width - InnerMargin * 2, Height - InnerMargin * 2, Color * InnerOpacity);
        for (int x = (int)Left + CornerSize + CornerSpace; x <= Right - (CornerSize + CornerSpace + LineSize); x += LineSize + SpaceSize)
        {
            Draw.Line(x, Top, x + LineSize, Top, Color);
            Draw.Line(x, Bottom - 1, x + LineSize, Bottom - 1, Color);
        }

        for (int y = (int)Top + CornerSize + CornerSpace; y + LineSize <= Bottom - (CornerSize + CornerSpace); y += LineSize + SpaceSize)
        {
            Draw.Line(Left + 1, y, Left + 1, y + LineSize, Color);
            Draw.Line(Right, y, Right, y + LineSize, Color);
        }

        Draw.Rect(Left, Top, CornerSize, CornerSize, Color);
        Draw.Rect(Right - CornerSize, Top, CornerSize, CornerSize, Color);
        Draw.Rect(Left, Bottom - CornerSize, CornerSize, CornerSize, Color);
        Draw.Rect(Right - CornerSize, Bottom - CornerSize, CornerSize, CornerSize, Color);
    }
}
#nullable restore
