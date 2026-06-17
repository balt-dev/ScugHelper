using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/GPUSpinner")]
[Tracked]
public class GPUSpinner : Entity
{
    internal readonly int RandomSeed;
    internal readonly bool Rainbow;
    internal readonly int ID;
    internal readonly List<MTexture> bgTex = [];
    internal readonly List<MTexture> fgTex = [];
    internal readonly Color Color = Color.White;
    internal Image? filler;
    internal Image? crystal;

    public GPUSpinner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        ID = id.ID;
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        RandomSeed = Calc.Random.Next();
        Add(new PlayerCollider(static player => player.Die(-player.Speed.SafeNormalize(Vector2.UnitY))));
        Rainbow = data.Bool("Rainbow");
        if (Rainbow)
            Tag |= Tags.TransitionUpdate;
        Color = data.HexColor("Color", Color.White);
        var spriteDir = data.String("SpritePath", "danger/crystal");
        var spriteSuffix = data.String("SpriteSuffix", "_white");
        bgTex = GFX.Game.GetAtlasSubtextures(spriteDir + $"/bg{spriteSuffix}");
        fgTex = GFX.Game.GetAtlasSubtextures(spriteDir + $"/fg{spriteSuffix}");
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<GPUSpinnerRenderer>()?.Add(this);
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        CreateSpinnerSprites(scene);
    }
    public override void Render() {}

    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<GPUSpinnerRenderer>()?.Remove(this);
    }

    private void AddFiller(Vector2 offset) {
        filler = new(Calc.Random.Choose(bgTex)) {
            Position = offset,
            Rotation = Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f),
            Color = Color
        };
        filler.CenterOrigin();

        Add(filler);
    }

    private void CreateSpinnerSprites(Scene scene) {
        Calc.PushRandom(RandomSeed);
        foreach (GPUSpinner entity in scene.Tracker.GetEntities<GPUSpinner>()) {
            if (entity.ID < ID && (entity.Position - Position).LengthSquared() < 576f)
                AddFiller((Position + entity.Position) / 2f - Position);
        }

        MTexture mTexture = Calc.Random.Choose(fgTex);
        crystal = new Image(mTexture).SetOrigin(12, 12).SetColor(Color);
        Add(crystal);

        Calc.PopRandom();
    }
    
    internal Color GetHue(Vector2 position) {
        float num = 280f;
        float value = (position.Length() + Scene.TimeActive * 50f) % num / num;
        return Calc.HsvToColor(0.4f + Calc.YoYo(value) * 0.4f, 0.4f, 0.9f);
    }
    
    public override void Update() {
        base.Update();
        if (Rainbow) {
            crystal?.Color = GetHue(Position + crystal.Position);
            filler?.Color = GetHue(Position + filler.Position);
        }
    }
}

[Tracked]
class GPUSpinnerRenderer : Entity {

    [OnLoad]
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new GPUSpinnerRenderer());
    }

    readonly List<GPUSpinner> Spinners = [];

    public GPUSpinnerRenderer() : base() {
        Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
        Depth = -8500;
        Add(new BeforeRenderHook(BeforeRender));
    }

    internal void Add(GPUSpinner ent) {
        if (Spinners.Contains(ent)) return;
        Spinners.Add(ent);
    }

    internal void Remove(GPUSpinner ent) {
        Spinners.Remove(ent);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        buffer?.Dispose();
    }

    VirtualRenderTarget? buffer;

    public void BeforeRender() {
        if (Spinners.Count == 0) return;
        buffer ??= VirtualContent.CreateRenderTarget("GPUSpinnerRenderer", Utils.BufferWidth, Utils.BufferHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        var cam = (Scene as Level)!.Camera;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, cam.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        foreach (GPUSpinner spinner in Spinners)
            if (spinner.Visible)
                spinner.filler?.Render();
        foreach (GPUSpinner spinner in Spinners)
            if (spinner.Visible)
                spinner.crystal?.Render();

        Draw.SpriteBatch.End();
    }

    public override void Render() {
        base.Render();
        if (Spinners.Count == 0) return;
        if (buffer is null) return;
        var cam = (Scene as Level)!.Camera;

        GameplayRenderer.End();
        ScugHelperModule.OutlineWithBaseFX?.Parameters["TexelSize"].SetValue(new Vector2(1f / Utils.BufferWidth, 1f / Utils.BufferHeight));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.OutlineWithBaseFX, cam.Matrix);
        Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
}
