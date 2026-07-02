
using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/CustomDistortion")]
public class CustomDistortion : Entity {
    public readonly List<MTexture> Frames = [];
    public readonly List<MTexture> OverlayFrames = [];
    public readonly float Framerate;
    public readonly float OverlayFramerate;
    internal float Elapsed;
    internal int CurrentFrame;
    internal int CurrentOverlayFrame;

    public CustomDistortion(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Frames = GFX.Game.GetAtlasSubtextures("distortions/" + data.String("ImagePath"));
        OverlayFrames = data.String("OverlayPath") is {} str ? GFX.Game.GetAtlasSubtextures("distortions/" + str) : [];
        Add(new PixelDistortionRenderer(DistortRender, OverlayRender));
        Framerate = data.Float("Framerate");
        OverlayFramerate = data.Float("OverlayFramerate");
    }
    
    public override void Update() {
        Elapsed += Engine.DeltaTime;
        CurrentFrame = Math.Clamp((int) (Elapsed * Framerate) % Frames.Count, 0, Frames.Count - 1);
        if (OverlayFrames.Count > 0)
            CurrentOverlayFrame = Math.Clamp((int) (Elapsed * OverlayFramerate) % OverlayFrames.Count, 0, OverlayFrames.Count - 1);
    }
    
    public override void DebugRender(Camera camera) {
        MTexture tex = Frames[CurrentFrame];
        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Position + tex.DrawOffset, Color.White);
    }
    
    private void DistortRender() {
        if (Frames.Count == 0 || Engine.Commands.Open || DebugViewTrigger.ForceRenderDebug) return;
        MTexture tex = Frames[CurrentFrame];
        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Position + tex.DrawOffset, Color.White);
    }
    
    private void OverlayRender() {
        if (OverlayFrames.Count == 0) return;
        MTexture tex = OverlayFrames[CurrentOverlayFrame];
        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Position + tex.DrawOffset, Color.White);
    }
}

#if false
// Underlying HLSL code
float4 Distort(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 offsetPixel = tex2D(mapS, uv);
    int offsetX = (int) ((offsetPixel.r * 255 - 128) * offsetPixel.b);
    int offsetY = (int) ((offsetPixel.g * 255 - 128) * offsetPixel.b);
    float2 texelOffset = float2(((float) offsetX) / BufferSize.x, ((float) offsetY) / BufferSize.y);

    return tex2D(srcS, uv + texelOffset);
}
#endif