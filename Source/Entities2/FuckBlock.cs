using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Linq;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/FuckBlock")]
public class FuckBlock : Solid {
    internal class FuckBlockColliderList(FuckBlock self) : AbstractEntityColliderList(self) {
        protected override bool CheckEntity(Entity entity) {
            if (entity is Player player) {
                bool oldStateLock = player.StateMachine.Locked;
                if (self.IgnoreStateLock)
                    player.StateMachine.Locked = false;
                player.StateMachine.State = self.State;
                player.StateMachine.Locked = oldStateLock;
            }
            return true;
        }
    }

    protected readonly char tileType;
    protected readonly float width;
    protected readonly float height;
    protected readonly bool BlendIn;
    internal readonly int State;
    internal readonly bool IgnoreStateLock;

    public FuckBlock(EntityData data, Vector2 offset)
    : base(data.Position + offset, data.Width, data.Height, safe: false) {
        Depth = -12999;
        tileType = data.Char("tiletype", '3');
        BlendIn = data.Bool("blendin");
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        State = data.Int("State", 0);
        IgnoreStateLock = data.Bool("IgnoreStateLock");
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        TileGrid tileGrid;
        if (!BlendIn) {
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8).TileGrid;
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
        Collider = new FuckBlockColliderList(this);
    }
}
