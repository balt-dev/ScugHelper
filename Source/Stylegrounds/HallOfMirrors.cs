using System;
using System.Threading;
using Celeste.Mod.Backdrops;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ScugHelper.Stylegrounds;

[CustomBackdrop("ScugHelper/HallOfMirrors")]
public class HallOfMirrors(BinaryPacker.Element data) : Backdrop {
    public Vector2 Offset = new(data.AttrFloat("OffsetX"), data.AttrFloat("OffsetY"));
    public float Rotation = data.AttrFloat("Rotation") * Calc.DegToRad;
    public readonly Vector2 Parallax = new(data.AttrFloat("ParallaxX"), data.AttrFloat("ParallaxY"));
    public readonly Vector2 MySpeed = new(data.AttrFloat("SpeedX"), data.AttrFloat("SpeedY"));
    public readonly Vector2 Wrap = new(data.AttrFloat("WrapX"), data.AttrFloat("WrapY"));
    public readonly float Zoom = data.AttrFloat("Zoom");
    public readonly Color Tint = Calc.HexToColor(data.Attr("Color")) * data.AttrFloat("Opacity");
    public readonly float RotationSpeed = data.AttrFloat("RotationSpeed") * Calc.DegToRad;
    public readonly bool CaptureForegroundAndBloom = data.AttrBool("CaptureForegroundAndBloom");
    internal static VirtualRenderTarget? NoFGBackbuffer;
    internal static VirtualRenderTarget? FullBackbuffer;
    internal bool InitialCleared;

    public override void Update(Scene scene) {
        base.Update(scene);
        Offset += MySpeed * Engine.DeltaTime;
        Rotation += RotationSpeed * Engine.DeltaTime;
    }

    [OnLoad] internal static void LoadHooks() {
        using (
            new DetourConfigContext(
                new DetourConfig("ScugHelper").WithPriority(int.MinValue)
            ).Use()
        ) IL.Celeste.Level.Render += ILLevelRender;
    }

    [OnUnload] internal static void UnloadHooks() =>
        IL.Celeste.Level.Render -= ILLevelRender;

    private static void ILLevelRender(ILContext il) {
        ILCursor cur = new(il);
        // we have to be REALLY paranoid here because so so so many people hook Level.Render
        if (!cur.TryGotoNext(
            MoveType.After,
            static match => match.MatchCallOrCallvirt(typeof(Distort), nameof(Distort.Render))
        )) throw new Utils.HookException("Failed to hook Level.Render for HallOfMirrors styleground!");

        cur.EmitLdarg0();
        cur.EmitDelegate(SaveNoFGBuffer);
        
        if (!cur.TryGotoNext(
            MoveType.After,
            static match => match.MatchCallOrCallvirt(typeof(Glitch), nameof(Glitch.Apply))
        )) throw new Utils.HookException("Failed to hook Level.Render for HallOfMirrors styleground!");

        cur.EmitLdarg0();
        cur.EmitDelegate(SaveFullBuffer);
    }

    internal static void SaveNoFGBuffer(Level level) {
        if (
            NoFGBackbuffer is null ||
            NoFGBackbuffer.Width != GameplayBuffers.Level.Width ||
            NoFGBackbuffer.Height != GameplayBuffers.Level.Height
        ) {
            NoFGBackbuffer?.Dispose();
            NoFGBackbuffer = VirtualContent.CreateRenderTarget($"HallOfMirrorsNoFGBuffer", GameplayBuffers.Level.Width, GameplayBuffers.Level.Height);
        }
        Engine.Instance.GraphicsDevice.SetRenderTarget(NoFGBackbuffer);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(
            GameplayBuffers.Level,
            Vector2.Zero,
            Color.White
        );
        Draw.SpriteBatch.End();
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
    }
    
    internal static void SaveFullBuffer(Level level) {
        if (
            FullBackbuffer is null ||
            FullBackbuffer.Width != GameplayBuffers.Level.Width ||
            FullBackbuffer.Height != GameplayBuffers.Level.Height
        ) {
            FullBackbuffer?.Dispose();
            FullBackbuffer = VirtualContent.CreateRenderTarget($"HallOfMirrorsFullBuffer", GameplayBuffers.Level.Width, GameplayBuffers.Level.Height);
        }
        Engine.Instance.GraphicsDevice.SetRenderTarget(FullBackbuffer);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(
            GameplayBuffers.Level,
            Vector2.Zero,
            Color.White
        );
        Draw.SpriteBatch.End();
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public override void Render(Scene scene) {
        base.Render(scene);
        if (scene is not Level level) return;
        if (!Visible) return;
        VirtualRenderTarget? buffer = CaptureForegroundAndBloom ? FullBackbuffer : NoFGBackbuffer;
        if (buffer is null) return;
        if (!InitialCleared) {
            InitialCleared = true;
            Engine.Graphics.GraphicsDevice.Clear(level.BackgroundColor);
        }
        ScugHelperModule.HallOfMirrorsFX?.Parameters["BufferSize"]
            .SetValue(new Vector2(buffer.Width, buffer.Height));
        ScugHelperModule.HallOfMirrorsFX?.Parameters["Offset"]
            .SetValue(Offset + level.Camera.Position * Parallax);
        ScugHelperModule.HallOfMirrorsFX?.Parameters["Rotation"]
            .SetValue(Rotation);
        ScugHelperModule.HallOfMirrorsFX?.Parameters["Zoom"]
            .SetValue(Zoom);
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.PointWrap, DepthStencilState.None,
            RasterizerState.CullNone, ScugHelperModule.HallOfMirrorsFX,
            Matrix.Identity
        );
        Draw.SpriteBatch.Draw(buffer.Target, Vector2.Zero, null, Tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
}
