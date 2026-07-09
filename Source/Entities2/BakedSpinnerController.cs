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
        if (scene is not Level lv) return;
        Level = lv;
        if (
            Level.Bounds.Width > ScugHelperModule.Settings.BakedTextureSizeLimit * 1024 || 
            Level.Bounds.Height > ScugHelperModule.Settings.BakedTextureSizeLimit * 1024
        ) {
            LevelEnter.ErrorMessage = Dialog.Get("ScugHelper_postcard_toobigroom").Replace("((maxSize))", (ScugHelperModule.Settings.BakedTextureSizeLimit * 1024 / 8).ToString());
            Engine.Scene = new LevelEnter(Level.Session, false);
            return;
        }
    }

    int BakedSpinners;

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (scene is not Level lv) return;
        LevelOffset = lv.LevelOffset;
        Level = lv;

        BakedSpinners = 0;

        BakedTarget = VirtualContent.CreateRenderTarget($"BakedSpinners-{Level.Session.Level}", Level.Bounds.Width, Level.Bounds.Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(BakedTarget);
        if ((bool)DynamicData.For(Draw.SpriteBatch).Get("beginCalled")!)
            Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, Matrix.CreateTranslation(-Level.LevelOffset.X, -Level.LevelOffset.Y, 0f)
        );
        try {
            foreach (CrystalStaticSpinner spin in Level.Tracker.GetEntities<CrystalStaticSpinner>().ToArray()) {
                BakedSpinners++;
                Level.Add(new InvisibleSpinner(spin.Position, spin.Collider));
                spinnerLocations.Add(spin.Center);
                spin.Visible = true;
                if (spin is GPUSpinner gpuSpin)
                    BakeSpinner(gpuSpin);
                else
                    BakeSpinner(spin);
                spin.RemoveSelf();
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
            Level.Add(new InvisibleSpinner(spin.Position, spin.Collider));
            spinnerLocations.Add(spin.Center);
            spin.Visible = true;
            BakeSpinner(spin);
            spin.RemoveSelf();
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
            
        // This is awful.
        var filler = data.Get("filler");
        if (filler is not null) {
            var fData = DynamicData.For(filler);
            var listData = DynamicData.For(fData.Invoke("get_Fills")!);
            var count = (int)listData.Invoke("get_Count")!;
            List<int> l = [];
            for (int i = 0; i < count; i++) {
                var fill = listData.Invoke("get_Item", i)!;
                var fillData = DynamicData.For(fill);
                MTexture tex = (MTexture)fillData.Get("Texture")!;
                Color color = (Color)fillData.Get("Color")!;
                Vector2 position = (Vector2)fillData.Get("Position")!;
                float scaleFix = tex.ScaleFix;
                scaleFix *= (float)data.Get("ImageScale")!;
                Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
                Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + spin.Position, tex.ClipRect, color, 0f, origin, scaleFix, SpriteEffects.None, 0f);
            }
        }

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

    public override void Removed(Scene scene) {
        base.Removed(scene);
        Dispose();
    }

    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        Dispose();
    }

    public void Dispose() {
        BakedTarget?.Dispose();
        BakedTarget = null;
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
        if (ScugHelperModule.Settings.AlwaysBakeSpinners)
            self.Add(new BakedSpinnerController());
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
