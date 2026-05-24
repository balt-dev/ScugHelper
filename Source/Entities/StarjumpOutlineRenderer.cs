using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
public class StarjumpOutlineRenderer : Entity
{
    private static readonly int BufferWidth = 512;
    private static readonly int BufferHeight = 512;

    [OnLoad]
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new StarjumpOutlineRenderer());
    }

    readonly List<Entity> Entities = [];

    public StarjumpOutlineRenderer() : base() {
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

    StarjumpSpinnerColorController control;

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if ((control = scene.Tracker.GetEntity<StarjumpSpinnerColorController>()) is null)
            scene.Add(control = new StarjumpSpinnerColorController(Color.White));
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        buffer?.Dispose();
    }

    public override void Update() {
        base.Update();
        foreach (Entity entity in Entities)
            entity.Visible = false;
    }

    VirtualRenderTarget buffer;

    public void BeforeRender() {
        if (Entities.Count == 0) return;
        buffer ??= VirtualContent.CreateRenderTarget("starjump-outline-renderer", BufferWidth, BufferHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        var cam = (Scene as Level).Camera;

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
        if (buffer is not null) {
            var cam = (Scene as Level).Camera;

            GameplayRenderer.End();
            ScugHelperModule.OutlineFX.Parameters["TexelSize"].SetValue(new Vector2(1f / BufferWidth, 1f / BufferHeight));
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.OutlineFX, cam.Matrix);
            Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, control.Color, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
    }

    private void RenderBloom() {
        if (!control.Bloom) return;
        if (Entities.Count == 0) return;
        var cam = (Scene as Level).Camera;

        GameplayRenderer.End();
        ScugHelperModule.OutlineFX.Parameters["TexelSize"].SetValue(new Vector2(1f / BufferWidth, 1f / BufferHeight));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.OutlineFX, cam.Matrix);
        Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
}

[Tracked]
[CustomEntity("ScugHelper/StarjumpSpinnerColorController")]
public class StarjumpSpinnerColorController(Color color = default, bool rainbow = false, bool bloom = false) : Entity()
{
    public StarjumpSpinnerColorController(EntityData data, Vector2 position)
        : this(data.HexColor("Color", Color.White), data.Bool("Rainbow"), data.Bool("Bloom")) { }

    internal readonly Color ActualColor = color;
    internal readonly bool Rainbow = rainbow;
    internal readonly bool Bloom = bloom;

    public Color Color { get => Rainbow ? Calc.HsvToColor(0.4f + Calc.YoYo((Scene?.TimeActive ?? 0f) * 50f % 280 / 280) * 0.4f, 0.4f, 0.9f) : ActualColor; }
}
