using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

static class CameraExt {
    internal static bool ContainsPoint(this Camera self, Vector2 pos, float margin = 16f) =>
        self.Left - margin >= pos.X && 
        self.Right + margin <= pos.X && 
        self.Top - margin >= pos.Y && 
        self.Bottom + margin <= pos.Y;
}

[CustomEntity("ScugHelper/Polyline")]
public class Polyline(EntityData data, Vector2 offset) : Entity(data.Position + offset) {
    internal readonly Vector2[] Points = data.NodesWithPosition(offset);
    internal readonly Color LineColor = data.HexColor("Color", Color.White);
    internal readonly float LineWidth = data.Float("Width", 2f);
    internal readonly MTexture NodeTexture = GFX.Game[data.String("NodeTexture")];
    internal readonly bool TintNodeTexture = data.Bool("TintNodeTexture", true);
    
    

    public override void Render() {
        if (Scene is not Level level) return;
        var camera = level.Camera;
        if (camera.ContainsPoint(Position) || camera.ContainsPoint(Points[0]))
            Draw.Line(Position, Points[0], LineColor, LineWidth);
        for (int i = 0; i < Points.Length - 1; i++)
            if (camera.ContainsPoint(Points[i]) || camera.ContainsPoint(Points[i + 1]))
                Draw.Line(Points[i], Points[i + 1], LineColor, LineWidth);
        if (camera.ContainsPoint(Position))
            Draw.SpriteBatch.Draw(
                NodeTexture.Texture.Texture_Safe, Position + NodeTexture.DrawOffset - new Vector2(NodeTexture.Width / 2, NodeTexture.Height / 2),
                TintNodeTexture ? LineColor : Color.White
            );
        for (int i = 0; i < Points.Length; i++)
            if (camera.ContainsPoint(Points[i]))
                Draw.SpriteBatch.Draw(
                    NodeTexture.Texture.Texture_Safe, Points[i] + NodeTexture.DrawOffset - new Vector2(NodeTexture.Width / 2, NodeTexture.Height / 2),
                    TintNodeTexture ? LineColor : Color.White
                );
    }
}
