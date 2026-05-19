using System;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/NoTilesController")]
public class NoTilesController() : Entity()
{
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Autotiler.GenerateOverlay += OnGenerateOverlay;
        IL.Celeste.LevelLoader.LoadingThread += ILLoadingThread;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Autotiler.GenerateOverlay -= OnGenerateOverlay;
        IL.Celeste.LevelLoader.LoadingThread -= ILLoadingThread;
    }
    private static void ILLoadingThread(ILContext il)
    {
        ILCursor cur = new(il);

        ILLabel EndTiles = cur.DefineLabel();
        // Start of tile loading
        if (!cur.TryGotoNext(MoveType.After,
            match => match.MatchCallOrCallvirt<List<Rectangle>>(nameof(List<>.Clear))
        )) throw new Exception("Failed to find start of tile loading for NoTilesController.");
        cur.EmitLdarg0(); // LevelLoader
        cur.EmitLdloc0(); // mapData
        cur.EmitDelegate(CheckForAnyNTC);
        cur.EmitBrtrue(EndTiles);
        
        
        if (!cur.TryGotoNext(MoveType.AfterLabel,
            match => match.MatchLdloc(1),
            match => match.MatchLdfld<AreaData>(nameof(AreaData.OnLevelBegin)),
            match => match.MatchBrfalse(out _)
        )) throw new Exception("Failed to find end of tile loading for NoTilesController.");
        cur.MarkLabel(EndTiles);
    }

    internal static bool CheckForAnyNTC(LevelLoader loader, MapData mapData)
    {
        Level level = loader.Level;
        foreach (LevelData levelData in mapData.Levels)
        {
            foreach (EntityData entityData in levelData.Entities)
            {
                if (entityData.Name == "ScugHelper/NoTilesController")
                {
                    SetUpNoTiles(level, mapData);
                    return true;
                }
            }
        }
        return false;
    }

    private static void SetUpNoTiles(Level self, MapData mapData)
    {
        Vector2 position = new(mapData.TileBounds.X, mapData.TileBounds.Y);
        self.BgTiles = new BackgroundTiles(position, self.BgData = new VirtualMap<char>(0, 0, '0'));
        self.SolidTiles = new SolidTiles(position, self.SolidsData = new VirtualMap<char>(0, 0, '0'));
        new Entity(position).Add(self.FgTilesLightMask = new TileGrid(8, 8, 0, 0));
        self.FgTilesLightMask.Color = Color.Black;
        foreach (LevelData level in mapData.Levels) {
            GFX.BGAutotiler.LevelBounds.Add(new Rectangle(level.TileBounds.X - mapData.TileBounds.X, level.TileBounds.Y - mapData.TileBounds.Y, 0, 0));
            GFX.FGAutotiler.LevelBounds.Add(new Rectangle(level.TileBounds.X - mapData.TileBounds.X, level.TileBounds.Y - mapData.TileBounds.Y, 0, 0));
        }
    }
    
    private static Autotiler.Generated OnGenerateOverlay(On.Celeste.Autotiler.orig_GenerateOverlay orig, Autotiler self, char id, int x, int y, int tilesX, int tilesY, VirtualMap<char> mapData)
    {
        if (mapData.Columns == 0 || mapData.Rows == 0) {
            TileGrid tileGrid = new(8, 8, 0, 0);
            AnimatedTiles animatedTiles = new(0, 0, GFX.AnimatedTilesBank);
            return new Autotiler.Generated { TileGrid = tileGrid, SpriteOverlay = animatedTiles };
        }
        else
            return orig(self, id, x, y, tilesX, tilesY, mapData);
    }
}
