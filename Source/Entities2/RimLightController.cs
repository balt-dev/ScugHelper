using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using FMOD.Studio;
using System;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.Cil;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RimLightController")]
public class RimLightController : Entity {
    public readonly Color Tint;
    public readonly float TintAlpha;
    public readonly string? TintAlphaSlider;
    public readonly BlendState BlendState;
    public readonly Vector2 Offset;
    public readonly float AlphaCoefficient;
    /*public static readonly BlendState MultiplyBlendMode = new() {
        Name = "BlendState.ScugHelper.MultiplyMask",
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add,
    };*/

    internal float Alpha;

    public RimLightController(EntityData data, Vector2 _) : base()
    {
        Tint = data.HexColor("Tint", Color.Black);
        TintAlpha = data.Float("TintAlpha", 0.5f);
        TintAlphaSlider = data.String("TintAlphaSlider", null);
        Offset = new(data.Float("OffsetX"), data.Float("OffsetY"));
        AlphaCoefficient = data.Float("AlphaCoefficient");
        BlendState = new() {
            Name = "BlendState.ScugHelper.RimLightCustom",
            ColorSourceBlend = data.Enum<Blend>("ColorSourceBlend"),
            ColorDestinationBlend = data.Enum<Blend>("ColorDestinationBlend"),
            ColorBlendFunction = data.Enum<BlendFunction>("ColorBlendFunction"),
            AlphaSourceBlend = data.Enum<Blend>("AlphaSourceBlend"),
            AlphaDestinationBlend = data.Enum<Blend>("AlphaDestinationBlend"),
            AlphaBlendFunction = data.Enum<BlendFunction>("AlphaBlendFunction"),
        };
        Tag |= Tags.FrozenUpdate | Tags.TransitionUpdate | Tags.PauseUpdate;
    }

    public override void Update() {
        Alpha = TintAlpha;
        if (Scene is Level level && TintAlphaSlider is string slider) Alpha *= level.Session.GetSlider(slider);
        base.Update();
    }

    [OnLoad] internal static void LoadHooks() {
        using (
            new DetourConfigContext(
                new DetourConfig("ScugHelper").WithPriority(int.MinValue)
            ).Use()
        ) On.Celeste.GameplayRenderer.Render += OnGameplayRendererRender;
    }

    [OnUnload] internal static void UnloadHooks() =>
        On.Celeste.GameplayRenderer.Render -= OnGameplayRendererRender;

    private static void OnGameplayRendererRender(On.Celeste.GameplayRenderer.orig_Render orig, GameplayRenderer self, Scene scene) {
        orig(self, scene);
        if (scene.Tracker.GetEntity<RimLightController>() is not RimLightController rlc) return;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
            DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity
        );
        Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White * rlc.Alpha);
        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
        ScugHelperModule.BlankFX!.Parameters["AlphaCoefficient"].SetValue(rlc.AlphaCoefficient);
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, rlc.BlendState, SamplerState.PointWrap,
            DepthStencilState.None, RasterizerState.CullNone, ScugHelperModule.BlankFX, Matrix.Identity
        );
        Draw.SpriteBatch.Draw(GameplayBuffers.TempA, rlc.Offset, rlc.Tint);
        Draw.SpriteBatch.End();
    }
}
