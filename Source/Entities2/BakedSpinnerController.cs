using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoMod.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/BakedSpinnerController")]
public class BakedSpinnerController : Entity, IDisposable {
    public BakedSpinnerController() : base() {
        Logger.Log(nameof(ScugHelper), ".ctor for BakedSpinnerController called...");
        Depth = -8500;
        Visible = true;
        Active = false;
    }

    internal VirtualRenderTarget? BakedTarget;
    private Level? Level = null;
    private readonly List<Vector2> spinnerLocations = [];
    private Vector2 LevelOffset;

    public override void Added(Scene scene) {
        base.Added(scene);
        Logger.Log(nameof(ScugHelper), "Added for BakedSpinnerController called...");
        if (scene is not Level lv) return;
        Level = lv;
        if (Level.Bounds.Width > 4096 || Level.Bounds.Height > 4096) {
            LevelEnter.ErrorMessage = Dialog.Get("ScugHelper_postcard_toobigroom");
            Engine.Scene = new LevelEnter(Level.Session, false);
            return;
        }
    }

    int BakedSpinners;

    public override void Awake(Scene scene) {
        base.Awake(scene);
        Logger.Log(nameof(ScugHelper), "Awake for BakedSpinnerController called...");
        if (scene is not Level lv) return;
        LevelOffset = lv.LevelOffset;
        Level = lv;

        BakedSpinners = 0;

        BakedTarget = VirtualContent.CreateRenderTarget($"BakedSpinners-{Level.Session.Level}", Level.Bounds.Width, Level.Bounds.Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(BakedTarget);
        if ((bool)DynamicData.For(Draw.SpriteBatch).Get("beginCalled")!)
            Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        try {
            foreach (CrystalStaticSpinner spin in Level.Tracker.GetEntities<CrystalStaticSpinner>().ToArray()) {
                BakedSpinners++;
                spin.Position -= Level.LevelOffset;
                spinnerLocations.Add(spin.Center);
                spin.Visible = true;
                BakeSpinner(spin);
                spin.RemoveSelf();
                Level.Add(new InvisibleSpinner(spin.Position, spin.Collider));
            }
            foreach (GPUSpinner spin in Level.Tracker.GetEntities<GPUSpinner>().ToArray()) {
                BakedSpinners++;
                spin.Position -= Level.LevelOffset;
                spinnerLocations.Add(spin.Center);
                spin.Visible = true;
                BakeSpinner(spin);
                spin.RemoveSelf();
                Level.Add(new InvisibleSpinner(spin.Position, spin.Collider));
            }
            if (FrostHelperImports.IsLoaded)
                _BakeFrostHelperSpinners();
        } finally {
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
        }
        Logger.Log(nameof(ScugHelper), $"Baked {BakedSpinners} spinners!");
    }

    private void _BakeFrostHelperSpinners() {
        if (Level is null) return;
        foreach (FrostHelper.CustomSpinner spin in Level.Tracker.GetEntities<FrostHelper.CustomSpinner>().ToArray()) {
            BakedSpinners++;
            spin.Position -= Level.LevelOffset;
            spinnerLocations.Add(spin.Center);
            spin.Visible = true;
            BakeSpinner(spin);
            spin.RemoveSelf();
            Level.Add(new InvisibleSpinner(spin.Position, spin.Collider));
        }
    }

    private void BakeSpinner(CrystalStaticSpinner spin) {
        spin.CreateSprites();
        if (spin.color == CrystalColor.Rainbow)
            spin.UpdateHue();
        if (spin.filler is { })
            foreach (Image image in spin.filler.Components.GetAll<Image>())
                image.Render();
        foreach (Image image in spin.Components.GetAll<Image>())
            image.Render();
    }
    private void BakeSpinner(GPUSpinner spin) {
        spin.CreateSpinnerSprites();
        if (spin.Rainbow)
            spin.SetHue();

        foreach (Image image in spin.fillers)
            image.Render();
        spin.crystal!.Render();
    }
    private void BakeSpinner(FrostHelper.CustomSpinner spin) {
        var data = DynamicData.For(spin);
        data.Invoke("CreateSprites");
        if (spin.Rainbow)
            spin.UpdateHue();
        foreach (Image image in (List<Image>)data.Get("_images")!)
            image.Render();
    }

    public override void Render() {
        base.Render();
        if (BakedTarget is not { } bakedSpinners) return;
        if (Level is null) return;
        GameplayRenderer.End();
        ScugHelperModule.OutlineWithBaseFX?.Parameters["TexelSize"].SetValue(new Vector2(1f / Level.Bounds.Width, 1f / Level.Bounds.Height));
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.OutlineWithBaseFX, Level.Camera.Matrix);
        Draw.SpriteBatch.Draw(bakedSpinners, LevelOffset, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        foreach (Vector2 pos in spinnerLocations)
            Draw.Circle(pos, 2f, Color.Lime, 6);
    }

    // Make absolutely sure it's disposed
    public override void Removed(Scene scene) => Dispose();
    public override void SceneEnd(Scene scene) => Dispose();
    ~BakedSpinnerController() => Dispose();

    public void Dispose() {
        Logger.Log(nameof(ScugHelper), "Dispose for BakedSpinnerController called...");
        BakedTarget?.Dispose();
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.CrystalStaticSpinner.CreateSprites += OnCrystalStaticSpinnerCreateSprites;
        On.Celeste.Level.LoadLevel += OnLevelLoad;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.CrystalStaticSpinner.CreateSprites -= OnCrystalStaticSpinnerCreateSprites;
        On.Celeste.Level.LoadLevel -= OnLevelLoad;
    }
    
    private static void OnLevelLoad(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
        if (
            ScugHelperModule.Settings.ReplaceVanillaSpinners &&
            self.Bounds.Width <= 4096 && self.Bounds.Height <= 4096
        ) self.Add(new BakedSpinnerController());
        orig(self, playerIntro, isFromLoader);
    }

    private static void OnCrystalStaticSpinnerCreateSprites(On.Celeste.CrystalStaticSpinner.orig_CreateSprites orig, CrystalStaticSpinner self) {
        if (self is InvisibleSpinner) self.expanded = true;
        else orig(self);
    }
    
    internal class InvisibleSpinner : CrystalStaticSpinner {
        public InvisibleSpinner(Vector2 position, Collider collider): base(position, false, CrystalColor.Blue) {
            Position = position;
            Collider = collider;
            Active = true;
            Visible = false;
        }
        
        public override void Awake(Scene scene) {
            base.Awake(scene);
            Components.RemoveAll<Image>();
        }

        public override void Update() {
            Visible = true;
            base.Update();
            Visible = false;
            border?.RemoveSelf();
            border = null;
        }
    }
}
