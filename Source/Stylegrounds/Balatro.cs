using Celeste.Mod.Backdrops;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ScugHelper.Stylegrounds;

[CustomBackdrop("ScugHelper/Balatro")]
public class Balatro(BinaryPacker.Element data) : Backdrop {
    public float SpinEase = data.AttrFloat("SpinEase");
    public Color Tint = Calc.HexToColor(data.Attr("Tint")) * data.AttrFloat("TintOpacity");
    public Color Color1 = Calc.HexToColor(data.Attr("Color1")) * data.AttrFloat("Opacity1");
    public Color Color2 = Calc.HexToColor(data.Attr("Color2")) * data.AttrFloat("Opacity2");
    public Color Color3 = Calc.HexToColor(data.Attr("Color3")) * data.AttrFloat("Opacity3");
    public Vector2 Offset = new(data.AttrFloat("OffsetX"), data.AttrFloat("OffsetY"));
    public float Contrast = data.AttrFloat("Contrast");
    public float SpinAmount = data.AttrFloat("SpinAmount");
    public float TimeScale = data.AttrFloat("TimeScale");
    public float SpinTimeScale = data.AttrFloat("SpinTimeScale");
    public bool Add = data.AttrBool("Add");

    internal static readonly VirtualTexture UnfuckedRect = new("ScugHelper.UnfuckedRect", 320, 180, Color.White);

    public override void Render(Scene scene) {
        base.Render(scene);
        if (scene is not Level level) return;
        if (!Visible) return;
        if (ScugHelperModule.BalatroFX is not {} fx) return;

        var cameraBounds = level.Camera.Bounds();

        fx.Parameters["BufferSize"].SetValue(new Vector2(cameraBounds.Width, cameraBounds.Height));
        fx.Parameters["Offset"].SetValue(Offset);
        fx.Parameters["Time"].SetValue(scene.TimeActive * TimeScale);
        fx.Parameters["SpinTime"].SetValue(scene.TimeActive * SpinTimeScale);
        fx.Parameters["SpinEase"].SetValue(SpinEase);
        fx.Parameters["Color1"].SetValue(Color1.ToVector4());
        fx.Parameters["Color2"].SetValue(Color2.ToVector4());
        fx.Parameters["Color3"].SetValue(Color3.ToVector4());
        fx.Parameters["Contrast"].SetValue(Contrast);
        fx.Parameters["SpinAmount"].SetValue(SpinAmount);
        
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred, Add ? BlendState.Additive : BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None,
            RasterizerState.CullNone, fx,
            level.Camera.Matrix
        );
        var cameraCenter = new Vector2(cameraBounds.Width, cameraBounds.Height) / 2f;
        Draw.SpriteBatch.Draw(
            UnfuckedRect.Texture_Safe, level.Camera.Position + cameraCenter,
            null, Tint, 0f, cameraCenter, 1f / level.Camera.Zoom, SpriteEffects.None, 0f
        );
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
}
