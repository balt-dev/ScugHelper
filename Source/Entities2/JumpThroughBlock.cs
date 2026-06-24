using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Linq;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/JumpThroughBlock")]
public class JumpThroughBlock : Solid {
    internal class JumpThroughBlockColliderList(JumpThroughBlock self) : AbstractEntityColliderList(self) {
        protected override bool CheckEntity(Entity entity) =>
            SpeedAccessor.For(entity) is { } accessor && self.Direction switch {
                JTDirection.Up => accessor.Speed.Y >= 0,
                JTDirection.Down => accessor.Speed.Y <= 0,
                JTDirection.Left => accessor.Speed.X > 0,
                JTDirection.Right => accessor.Speed.X < 0,
            };
    }

    internal enum JTDirection { Up, Down, Left, Right }

    internal readonly char tileType;
    internal readonly bool BlendIn;
    internal readonly JTDirection Direction;

    public JumpThroughBlock(EntityData data, Vector2 offset)
    : base(data.Position + offset, data.Width, data.Height, safe: false) {
        Depth = -12999;
        tileType = data.Char("tiletype", '3');
        BlendIn = data.Bool("blendin");
        Direction = data.Enum<JTDirection>("Direction");
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        TileGrid tileGrid;
        if (!BlendIn) {
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            Add(new LightOcclude());
        } else {
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)(X / 8f) - tileBounds.Left;
            int y = (int)(Y / 8f) - tileBounds.Top;
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(new EffectCutout());
            Depth = -10501;
        }

        Add(tileGrid);
        Add(new TileInterceptor(tileGrid, highPriority: true));
        if (CollideCheck<Player>())
            RemoveSelf();
        Collider = new JumpThroughBlockColliderList(this);
    }
}
