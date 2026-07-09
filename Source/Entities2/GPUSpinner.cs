using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/GPUSpinner")]
[TrackedAs(typeof(CrystalStaticSpinner))]
public class GPUSpinner : CrystalStaticSpinner {
    internal readonly int RandomSeed;
    internal readonly int ID;
    internal readonly List<MTexture> bgTex = [];
    internal readonly List<MTexture> fgTex = [];
    internal readonly List<Image> fillers = [];
    internal Image? crystal;
    
    // for mod interop
    public readonly Rectangle CullingRect;
    public readonly bool Rainbow;
    public readonly Color Color = Color.White;
    
    private readonly string SpriteDirectory;
    private readonly string SpriteSuffix;
    private readonly Color ShatterColor;

    public IReadOnlyList<Image> Fillers => fillers;
    public Image? Crystal => crystal;

    internal Vector2 Shake;

    public GPUSpinner(EntityData data, Vector2 offset, EntityID id) : base(data, offset, CrystalColor.Purple) {
        this.offset = Calc.Random.NextFloat();
        ID = id.ID;
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        RandomSeed = Calc.Random.Next();
        Add(new PlayerCollider(static player => player.Die(-player.Speed.SafeNormalize(Vector2.UnitY))));
        Rainbow = data.Bool("Rainbow", false);
        Tag |= Tags.TransitionUpdate;
        Color = data.HexColor("Color", Color.White);
        ShatterColor = data.HexColor("ShatterColor", Color);
        SpriteDirectory = data.String("SpritePath", "danger/crystal");
        SpriteSuffix = data.String("SpriteSuffix", "");
        bgTex = GFX.Game.GetAtlasSubtextures(SpriteDirectory + $"/bg{SpriteSuffix}");
        fgTex = GFX.Game.GetAtlasSubtextures(SpriteDirectory + $"/fg{SpriteSuffix}");
        CullingRect = new(64, 64, (int) Position.X - 32, (int) Position.Y - 32);
        expanded = true;
        Components.RemoveAll<StaticMover>();
        if (AttachToSolid) {
            Add(new StaticMover {
                OnShake = (offset) => Shake += offset,
                SolidChecker = IsRiding,
                OnDestroy = RemoveSelf
            });
        }
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    private void EntityUpdate() { }
    [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
    private void EntityAdded(Scene scene) { }
    [MonoModLinkTo("Monocle.Entity", "System.Void Awake(Monocle.Scene)")]
    private void EntityAwake(Scene scene) { }
    [MonoModLinkTo("Monocle.Entity", "System.Void Removed(Monocle.Scene)")]
    private void EntityRemoved(Scene scene) { }
    
    public override void Added(Scene scene) {
        EntityAdded(scene);
        if (scene.Tracker.GetEntity<BakedSpinnerController>() is null)
            scene.Tracker.GetEntity<GPUSpinnerRenderer>()?.Add(this);
    }
    public override void Awake(Scene scene) {
        EntityAwake(scene);
        CreateSpinnerSprites();
    }
    public override void Render() {}

    public override void Removed(Scene scene) {
        EntityRemoved(scene);
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

    internal void CreateSpinnerSprites() {
        if (CreatedSprites) return;
        CreatedSprites = true;
        Calc.PushRandom(RandomSeed);

        MTexture crys = Calc.Random.Choose(fgTex);

        foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
            if (entity is GPUSpinner spin && spin.ID > ID && (entity.Position - Position).LengthSquared() < 576f)
                AddFiller((Position + entity.Position) / 2f - Position);

        crystal = new Image(crys).SetOrigin(12, 12).SetColor(Color);
        crystal.Entity = this;
        if (Rainbow) SetHue();

        Calc.PopRandom();
    }

    public override void Update() {
        EntityUpdate();
        if (Get<RefillCrystal.Marker>() is {}) return;
        
        Visible = InView();
        if (Rainbow && Visible) SetHue();

        if (
            Scene.OnInterval(0.05f, offset) &&
            Scene.Tracker.GetEntity<Player>() is Player player
        ) Collidable = Math.Abs(player.X - X) < 128f && Math.Abs(player.Y - Y) < 128f;
    }

    internal void SetHue() {
        crystal?.Color = GetHue(Position + crystal.Position);
        for (int i = 0; i < fillers.Count; i++)
            fillers[i]?.Color = GetHue(Position + fillers[i].Position);
    }
    
    [OnLoad] internal static void LoadHooks() => IL.Celeste.CrystalStaticSpinner.Destroy += OnCrystalStaticSpinnerDestroy;
    [OnUnload] internal static void UnloadHooks() => IL.Celeste.CrystalStaticSpinner.Destroy -= OnCrystalStaticSpinnerDestroy;

    private static void OnCrystalStaticSpinnerDestroy(ILContext il) {
        ILCursor cur = new(il);
        if (
            !cur.TryGotoNext(MoveType.AfterLabel, static match => match.MatchCallOrCallvirt<CrystalDebris>(nameof(CrystalDebris.Burst))) || 
            !cur.TryGotoPrev(MoveType.After, static match => match.MatchLdloc0())
        ) throw new Utils.HookException("Failed to hook CrystalStaticSpinner.Destroy for GPU spinners!");
        cur.EmitLdarg0();
        cur.EmitDelegate(ReplaceColor);

        static Color ReplaceColor(Color origColor, CrystalStaticSpinner spin) =>
            spin is GPUSpinner gpuSpin 
                ? ( gpuSpin.Rainbow 
                    ? (gpuSpin.Crystal?.Color ?? Color.White)
                    : gpuSpin.ShatterColor
                ) : origColor;
    }
}

[Tracked]
class GPUSpinnerRenderer : Entity {

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
        Dispose();
    }

    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        Dispose();
    }

    public void Dispose() {
        buffer?.Dispose();
        buffer = null;
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
                    filler.RenderPosition + filler.Texture.DrawOffset + spinner.Shake, filler.Texture.ClipRect,
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
                crystal.RenderPosition + crystal.Texture.DrawOffset + spinner.Shake, crystal.Texture.ClipRect,
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
        Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
    
    [OnLoad]
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
        Everest.Events.Level.OnLoadEntity += OnEverestLevelOnLoadEntity;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
        Everest.Events.Level.OnLoadEntity -= OnEverestLevelOnLoadEntity;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new GPUSpinnerRenderer());
    }

    private static bool OnEverestLevelOnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
        if (ScugHelperModule.Settings.ReplaceVanillaSpinners && entityData.Name == "spinner") {
            if (level.Session.Area.ID == 3 || (level.Session.Area.ID == 7 && level.Session.Level.StartsWith("d-")))
                return false;
            string? customColor = entityData.Attr("color", null);
            if (customColor is not null && (customColor.IsWhiteSpace() || customColor.Length == 0))
                customColor = null;
            customColor = customColor?.ToLowerInvariant();
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
