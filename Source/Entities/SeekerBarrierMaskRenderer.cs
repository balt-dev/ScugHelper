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

    readonly List<Entity> Entities = [];

    public SeekerBarrierMaskRenderer() : base() {
        Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
        Depth = -8500;
        Add(new BeforeRenderHook(BeforeRender));
        Add(new CustomBloom(RenderBloom));
    }

    internal void Track(Entity ent) {
        if (Entities.Contains(ent)) return;
        ent.Visible = false;
        Entities.Add(ent);
    }

    internal void Untrack(Entity ent) {
        Entities.Remove(ent);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        buffer?.Dispose();
    }

    float Elapsed = 0f;

    public override void Update() {
        base.Update();
        Elapsed += Engine.DeltaTime;
        foreach (Entity entity in Entities)
            entity.Visible = false;
    }

    VirtualRenderTarget? buffer;

    public void BeforeRender() {
        if (Entities.Count == 0) return;
        buffer ??= VirtualContent.CreateRenderTarget("SeekerBarrierMaskRenderer", Utils.BufferWidth, Utils.BufferHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        var cam = (Scene as Level)!.Camera;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, cam.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        foreach (Entity entity in Entities) {
            entity.Visible = true;
            entity.Render();
            entity.Visible = false;
        }

        Draw.SpriteBatch.End();
    }

    public override void Render() {
        base.Render();
        if (Entities.Count == 0) return;
        if (buffer is null) return;
        var cam = (Scene as Level)!.Camera;

        GameplayRenderer.End();
        ScugHelperModule.SeekerBarrierFX?.Parameters["ParticleColor"].SetValue(Vector4.One * 0.5f);
        ScugHelperModule.SeekerBarrierFX?.Parameters["CameraPosition"].SetValue(cam.Position);
        ScugHelperModule.SeekerBarrierFX?.Parameters["TexelSize"].SetValue(new Vector2(1f / Utils.BufferWidth, 1f / Utils.BufferHeight));
        ScugHelperModule.SeekerBarrierFX?.Parameters["ActiveTime"].SetValue(Elapsed);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.SeekerBarrierFX, cam.Matrix);
        Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White * 0.15f, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }

    private void RenderBloom() {
        if (buffer is null) return;
        if (Entities.Count == 0) return;
        var cam = (Scene as Level)!.Camera;
        Draw.SpriteBatch.Draw(buffer.Target, cam.Position.Rounded(), null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
    }
}
