using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
public abstract class AbstractBreakableBlock : Solid
{
    protected readonly char tileType;
    protected readonly bool permanent;
    protected readonly float width;
    protected readonly float height;
    protected readonly bool blendIn;
    protected readonly EntityID id;

    public AbstractBreakableBlock(Vector2 position, char tiletype, float width, float height, bool blendIn, bool permanent, EntityID id)
        : base(position, width, height, safe: true)
    {
        DashBlock _;
        Depth = -12999;
        tileType = tiletype;
        this.id = id;
        this.permanent = permanent;
        this.width = width;
        this.height = height;
        this.blendIn = blendIn;
        tileType = tiletype;
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
    }
    public AbstractBreakableBlock(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("blendin"), data.Bool("permanent", true), id)
    {
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        TileGrid tileGrid;
        if (!blendIn)
        {
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8).TileGrid;
            Add(new LightOcclude());
        }
        else
        {
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
    
    
    public void Break(Vector2 from, bool playSound = true, bool playDebrisSound = true)
    {
        if (playSound)
        {
            if (tileType == '1')
                Audio.Play("event:/game/general/wall_break_dirt", Position);
            else if (tileType == '3')
                Audio.Play("event:/game/general/wall_break_ice", Position);
            else if (tileType == '9')
                Audio.Play("event:/game/general/wall_break_wood", Position);
            else
                Audio.Play("event:/game/general/wall_break_stone", Position);
        }

        for (int i = 0; i < Width / 8f; i++)
        {
            for (int j = 0; j < Height / 8f; j++)
            {
                Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
            }
        }

        Collidable = false;
        if (permanent)
            RemoveAndFlagAsGone();
        else
            RemoveSelf();
    }
    public void RemoveAndFlagAsGone()
    {
        RemoveSelf();
        SceneAs<Level>().Session.DoNotLoad.Add(id);
    }
}
#nullable restore
