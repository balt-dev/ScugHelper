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
    private readonly EntityID ID;
    private VirtualRenderTarget? bakedTexture;

    public Outline(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        ID = id;
        Collidable = false;
        Collider = new Hitbox(data.Width, data.Height);
        Depth = data.Int("Depth", 10);
        Color = data.HexColor("Color", Color.White);
        InnerOpacity = data.Float("InnerOpacity", 0.25f);
        LineSize = Math.Max(data.Int("LineSize", 2), 0);
        SpaceSize = Math.Max(data.Int("SpaceSize", 1), 0);
        if (SpaceSize + LineSize <= 0) {
            LineSize = 0;
            SpaceSize = 1;
        }
        CornerSize = data.Int("CornerSize", 2);
        CornerSpace = data.Int("CornerSpace", 1);
        InnerMargin = data.Int("InnerMargin", 4);
        Add(new BeforeRenderHook(BakeTexture));
    }
    
    internal void BakeTexture() {
        if (bakedTexture is null) {
            var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(bakedTexture = VirtualContent.CreateRenderTarget($"outlinePrerender_{ID}", (int)Width, (int)Height));

            Draw.SpriteBatch.Begin();
                    
            Draw.Rect(InnerMargin, InnerMargin, Width - InnerMargin * 2, Height - InnerMargin * 2, Color * InnerOpacity);
            for (int x = CornerSize + CornerSpace; x <= Width - (CornerSize + CornerSpace + LineSize); x += LineSize + SpaceSize) {
                Draw.Line(x, 0, x + LineSize, 0, Color);
                Draw.Line(x, Height - 1, x + LineSize, Height - 1, Color);
            }
    
            for (int y = CornerSize + CornerSpace; y + LineSize <= Height - (CornerSize + CornerSpace); y += LineSize + SpaceSize) {
                Draw.Line(1, y, 1, y + LineSize, Color);
                Draw.Line(Width, y, Width, y + LineSize, Color);
            }
    
            Draw.Rect(0, 0, CornerSize, CornerSize, Color);
            Draw.Rect(Width - CornerSize, 0, CornerSize, CornerSize, Color);
            Draw.Rect(0, Height - CornerSize, CornerSize, CornerSize, Color);
            Draw.Rect(Width - CornerSize, Height - CornerSize, CornerSize, CornerSize, Color);
            Draw.SpriteBatch.End();
            
            Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        }
    }

    public override void Render() {
        base.Render();
        if (bakedTexture is not null) Draw.SpriteBatch.Draw(bakedTexture, Position, Color.White);   
    }
    
    public override void Removed(Scene scene) {
        base.Removed(scene);
        bakedTexture?.Dispose();
    }
}
#nullable restore
