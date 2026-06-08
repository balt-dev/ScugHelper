using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SeekerBarrierMaskRenderer")]
public class SeekerBarrierMaskRenderer : Entity
{

    [OnLoad]
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new SeekerBarrierMaskRenderer());
    }

    static readonly float FieldOpacity = 0.15f;

    readonly List<Entity> Entities = [];

    protected static readonly float[] speeds = [12f, 20f, 40f];
    private static readonly int BufferWidth = 512;
    private static readonly int BufferHeight = 512;
    private static readonly int ParticleWidth = 128;
    private static readonly int ParticleHeight = 128;
    protected static readonly Vector2[] particles = new Vector2[ParticleWidth * ParticleHeight / 64];

    static SeekerBarrierMaskRenderer() {
        for (int i = 0; i < particles.Length; i++)
            particles[i] = new Vector2(Calc.Random.NextFloat(ParticleWidth - 1f), Calc.Random.NextFloat(ParticleHeight - 1f));
    }

    public SeekerBarrierMaskRenderer() : base() {
        Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
        Depth = -8500;
        Add(new CustomBloom(OnRenderBloom));
    }

    internal void Track(Entity ent) {
        if (Entities.Contains(ent)) return;
        ent.Visible = false;
        Entities.Add(ent);
    }

    internal void Untrack(Entity ent) {
        Entities.Remove(ent);
    }

    Vector2 lastCamPosition;

    public override void Update() {
        base.Update();
        if (Entities.Count == 0) return;
        var newCamPosition = SceneAs<Level>().Camera.Position;
        var deltaCam = newCamPosition - lastCamPosition;
        int count = particles.Length;
        for (int i = 0; i < count; i++) {
            Vector2 value = particles[i] - deltaCam + Vector2.UnitY * speeds[i % speeds.Length] * Engine.DeltaTime;
            value.X = (value.X % ParticleWidth + ParticleWidth) % ParticleWidth;
            value.Y = (value.Y % ParticleHeight + ParticleHeight) % ParticleHeight;
            particles[i] = value;
        }
        lastCamPosition = newCamPosition;
        foreach (Entity entity in Entities)
            entity.Visible = false;
    }

    static VirtualRenderTarget? DrawBuffer;
    static VirtualRenderTarget? MaskBuffer;

    static readonly Vector2[] SquareVerts = [Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY];

    public override void Render() {
        base.Render();
        if (Entities.Count == 0) return;
        var cam = (Scene as Level)!.Camera;
        DrawBuffer ??= VirtualContent.CreateRenderTarget("SBMRScratchDraw", BufferWidth, BufferHeight);
        MaskBuffer ??= VirtualContent.CreateRenderTarget("SBMRScratchMask", BufferWidth, BufferHeight);

        var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();
        GameplayRenderer.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskBuffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, cam.Matrix);
        foreach (Entity entity in Entities) {
            entity.Visible = true;
            entity.Render();
            entity.Visible = false;
        }
        Draw.SpriteBatch.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(DrawBuffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        DrawParticles();
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Utils.AlphaMaskBlendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(MaskBuffer, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, cam.Matrix);
        Draw.SpriteBatch.Draw(MaskBuffer, cam.Position.Rounded(), null, Color.White * FieldOpacity, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        Draw.SpriteBatch.Draw(DrawBuffer, cam.Position.Rounded(), null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }

    private void DrawParticles() {
        // TODO: REPLACE THIS WITH A SHADER. THIS HURTS PERFORMANCE A LOT
        
        for (int i = 0; i < particles.Length; i++) {
            Vector2 part = particles[i];
            for (int x = 0; x < (BufferWidth / ParticleWidth); x++)
                for (int y = 0; y < (BufferHeight / ParticleHeight); y++)
                    Draw.Pixel.Draw(part + new Vector2(x * ParticleWidth, y * ParticleHeight), Vector2.Zero, Color.White * 0.5f);
        }
    }

    private void OnRenderBloom() {
        if (Entities.Count == 0) return;
        if (MaskBuffer is null) return;
        var cam = (Scene as Level)!.Camera;
        Draw.SpriteBatch.Draw(MaskBuffer, cam.Position.Rounded(), null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
    }
}
