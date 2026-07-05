using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

static class CameraExt {
    internal static bool ContainsPoint(this Camera self, Vector2 pos, float margin = 16f) =>
        self.Left - margin <= pos.X && 
        self.Right + margin >= pos.X && 
        self.Top - margin <= pos.Y && 
        self.Bottom + margin >= pos.Y;
}

[CustomEntity("ScugHelper/Polyline")]
public class Polyline : Entity {
    internal readonly Vector2[] Points;
    internal readonly Color LineColor;
    internal readonly Color FadeColor;
    internal readonly string FadeSlider;
    internal readonly float LineWidth;
    internal readonly string Flag;
    internal readonly bool FlagState;
    internal readonly MTexture NodeTexture;
    internal readonly bool TintNodeTexture;

    public Polyline(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = data.Int("Depth");
        Points = data.NodesWithPosition(offset);
        LineColor = data.HexColor("Color", Color.White);
        FadeColor = data.HexColor("Color2", LineColor);
        FadeSlider = data.String("ColorFadeSlider", "");
        LineWidth = data.Float("Width", 2f);
        NodeTexture = GFX.Game[data.String("NodeTexture")];
        TintNodeTexture = data.Bool("TintNodeTexture", true);
        Flag = data.String("Flag");
        FlagState = data.Bool("FlagState", true);
    }

    public override void Render() {
        if (Scene is not Level level) return;
        if (Flag is not null && level.Session.GetFlag(Flag) != FlagState) return;
        var a = LineColor.ToHsv();
        var b = FadeColor.ToHsv();
        var hsvColor = a + (b - a) * level.Session.GetSlider(FadeSlider);
        var color = Calc.HsvToColor(hsvColor.X, hsvColor.Y, hsvColor.Z);
        var camera = level.Camera;
        if (camera.ContainsPoint(Position) || camera.ContainsPoint(Points[0]))
            Draw.Line(Position, Points[0], color, LineWidth);
        for (int i = 0; i < Points.Length - 1; i++)
            if (camera.ContainsPoint(Points[i]) || camera.ContainsPoint(Points[i + 1]))
                Draw.Line(Points[i], Points[i + 1], color, LineWidth);
        if (camera.ContainsPoint(Position))
            Draw.SpriteBatch.Draw(
                NodeTexture.Texture.Texture_Safe, Position + NodeTexture.DrawOffset - new Vector2(NodeTexture.Width / 2, NodeTexture.Height / 2),
                TintNodeTexture ? color : Color.White
            );
        for (int i = 0; i < Points.Length; i++)
            if (camera.ContainsPoint(Points[i]))
                Draw.SpriteBatch.Draw(
                    NodeTexture.Texture.Texture_Safe, Points[i] + NodeTexture.DrawOffset - new Vector2(NodeTexture.Width / 2, NodeTexture.Height / 2),
                    TintNodeTexture ? color : Color.White
                );
    }
}
