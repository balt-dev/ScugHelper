using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/InstantFlagBlock")]
public class InstantFlagBlock: Solid
{
    protected readonly char tileType;
    protected readonly bool blendIn;
    protected readonly string? flag;
    protected readonly bool state;

    public InstantFlagBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, data.Bool("Safe")) {
        Depth = -12999;
        blendIn = data.Bool("blendin");
        flag = data.String("Flag");
        state = data.Bool("FlagState");
        tileType = data.Char("tiletype", '3');
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
    }
    
    public override void Awake(Scene scene) {
        base.Awake(scene);
        FlagCheck();
        TileGrid tileGrid;
        if (!blendIn) {
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
    }
    
    public override void Update() {
        base.Update();
        FlagCheck();
    }

    private void FlagCheck() => Collidable = Visible = flag is null || (SceneAs<Level>().Session.GetFlag(flag) == state);
}
#nullable restore
