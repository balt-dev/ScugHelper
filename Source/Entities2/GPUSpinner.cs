using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/GPUSpinner")]
[Tracked]
public class GPUSpinner : Entity {
    internal readonly int RandomSeed;
    internal readonly bool Rainbow;
    internal readonly int ID;
    internal readonly List<MTexture> bgTex = [];
    internal readonly List<MTexture> fgTex = [];
    internal readonly Color Color = Color.White;
    internal readonly Rectangle CullingRect;
    internal readonly List<Image> fillers = [];
    internal Image? crystal;
    internal float offset;

    public GPUSpinner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        this.offset = Calc.Random.NextFloat();
        ID = id.ID;
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        RandomSeed = Calc.Random.Next();
        Add(new PlayerCollider(static player => player.Die(-player.Speed.SafeNormalize(Vector2.UnitY))));
        Rainbow = data.Bool("Rainbow");
        Tag |= Tags.TransitionUpdate;
        Color = data.HexColor("Color", Color.White);
        var spriteDir = data.String("SpritePath", "danger/crystal");
        var spriteSuffix = data.String("SpriteSuffix", "_white");
        bgTex = GFX.Game.GetAtlasSubtextures(spriteDir + $"/bg{spriteSuffix}");
        fgTex = GFX.Game.GetAtlasSubtextures(spriteDir + $"/fg{spriteSuffix}");
        CullingRect = new(64, 64, (int) Position.X - 32, (int) Position.Y - 32);
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Tracker.GetEntity<GPUSpinnerRenderer>()?.Add(this);
    }
    public override void Render() {}

    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Tracker.GetEntity<GPUSpinnerRenderer>()?.Remove(this);
    }

    private void AddFiller(Vector2 offset) {
        Image filler = new(Calc.Random.Choose(bgTex)) {
            Position = offset,
            Rotation = Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f),
            Color = Color
        };
        filler.CenterOrigin();

        fillers.Add(filler);
        filler.Entity = this;
    }

    private bool CreatedSprites;

    private void CreateSpinnerSprites() {
        if (CreatedSprites) return;
        CreatedSprites = true;
        Calc.PushRandom(RandomSeed);

        MTexture crys = Calc.Random.Choose(fgTex);

        foreach (GPUSpinner entity in Scene.Tracker.GetEntities<GPUSpinner>())
            if (entity.ID > ID && (entity.Position - Position).LengthSquared() < 576f)
                AddFiller((Position + entity.Position) / 2f - Position);

        crystal = new Image(crys).SetOrigin(12, 12).SetColor(Color);
        crystal.Entity = this;

        Calc.PopRandom();
    }

    public override void Update() {
        base.Update();
        CreateSpinnerSprites();
        if (Rainbow && Scene.OnInterval(0.08f, offset)) {
            crystal?.Color = GetHue(Position + crystal.Position);
            for (int i = 0; i < fillers.Count; i++)
                fillers[i]?.Color = GetHue(Position + fillers[i].Position);
        }

        if (Scene.OnInterval(0.25f, offset) && !InView())
            Visible = false;

        if (
            Scene.OnInterval(0.05f, offset) &&
            Scene.Tracker.GetEntity<Player>() is Player player
        ) Collidable = Math.Abs(player.X - X) < 128f && Math.Abs(player.Y - Y) < 128f;
    }

    private bool InView() => (Scene as Level)?.Camera.Bounds()
        .Intersection(CullingRect) is not null;

    internal Color GetHue(Vector2 position) {
        float num = 280f;
        float value = (position.Length() + Scene.TimeActive * 50f) % num / num;
        return Calc.HsvToColor(0.4f + Calc.YoYo(value) * 0.4f, 0.4f, 0.9f);
    }

    [OnLoad]
    public static void LoadHooks() =>
        Everest.Events.Level.OnLoadEntity += OnEverestLevelOnLoadEntity;

    [OnUnload]
    public static void UnloadHooks() =>
        Everest.Events.Level.OnLoadEntity -= OnEverestLevelOnLoadEntity;

    private static bool OnEverestLevelOnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
        if (ScugHelperModule.Settings.ReplaceVanillaSpinners && entityData.Name == "spinner") {
            if (level.Session.Area.ID == 3 || (level.Session.Area.ID == 7 && level.Session.Level.StartsWith("d-")))
                return false;
            string? customColor = entityData.Attr("color", null);
            entityData.Name = "ScugHelper/GPUSpinner";
            entityData.Values["SpritePath"] = "danger/crystal";
            entityData.Values["Color"] = "FFFFFF";
            if (customColor is "rainbow" || (customColor is null && level.Session.Area.ID is 10)) {
                entityData.Values["SpriteSuffix"] = "_white";
                entityData.Values["Rainbow"] = true;
                Logger.Log(nameof(ScugHelper), $"Replacing vanilla spinner! Color: {customColor}, rainbow: true");
                level.Add(new GPUSpinner(entityData, offset, new(levelData.Name, entityData.ID)));
                return true;
            }
            if (customColor is "core") customColor = "red";
            customColor ??= level.Session.Area.ID switch {
                5 => "red",
                6 => "purple",
                _ => "blue",
            };
            Logger.Log(nameof(ScugHelper), $"Replacing vanilla spinner! Color: {customColor}, rainbow: false");
            entityData.Values["SpriteSuffix"] = "_" + customColor;
            level.Add(new GPUSpinner(entityData, offset, new(levelData.Name, entityData.ID)));
            return true;
        }
        return false;
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
        buffer ??= VirtualContent.CreateRenderTarget("GPUSpinnerRenderer", Utils.BufferWidth, Utils.BufferHeight);
        if (Spinners.Count == 0) return;
        var cam = (Scene as Level)!.Camera;

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, cam.Matrix);
        
        var spinners = CollectionsMarshal.AsSpan(Spinners);

        for (int i = 0; i < spinners.Length; i++)
        {
            var spinner = spinners[i];
            if (!spinner.Visible) continue;
            var fillers = CollectionsMarshal.AsSpan(spinner.fillers);
            for (int j = 0; j < fillers.Length; j++) {
                var filler = spinner.fillers[j];

                Draw.SpriteBatch.Draw(
                    filler.Texture.Texture.Texture_Safe,
                    filler.RenderPosition, filler.Texture.ClipRect,
                    filler.Color, 0f,
                    filler.Origin, 1f, SpriteEffects.None, 0f
                );
            }
        }
        for (int i = 0; i < spinners.Length; i++) {
            var spinner = spinners[i];
            if (!spinner.Visible) continue;
            var crystal = spinner.crystal;
            if (crystal is null) continue;

            Draw.SpriteBatch.Draw(
                crystal.Texture.Texture.Texture_Safe,
                crystal.RenderPosition, crystal.Texture.ClipRect,
                crystal.Color, 0f,
                crystal.Origin, 1f, SpriteEffects.None, 0f
            );
        }

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
