using System;
using System.IO;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using KeraLua;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Celeste.Mod.ScugHelper.Entities.Actions;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/ProceduralTilemap")]
public class ProceduralTilemap : SolidTiles
{

    private readonly EntityID ID;
    public readonly int TileWidth;
    public readonly int TileHeight;
    public TileGrid BGGrid { get; internal set; }
    public AnimatedTiles BGAnim { get; internal set; }
    public readonly bool AbsoluteX;
    public readonly bool AbsoluteY;
    public readonly bool MoveWithPlayer;
    public const float RegenerateMarginX = 30 * 8 + 40;
    public const float RegenerateMarginY = 16 * 8 + 23;
    readonly string[] Arguments;

    Rectangle Bounds => new((int)X, (int)Y, TileWidth * 8, TileHeight * 8);

    int XOffset;
    int YOffset;

    readonly string filePath;
    LuaCallbacks callbacks;

    struct LuaCallbacks {
        internal int? beforeFuncRef;
        internal int? foregroundFuncRef;
        internal int? backgroundFuncRef;
    }

    static readonly Dictionary<EntityID, LuaCallbacks> CallbackCache = [];
    private readonly Hitbox LeftCollider;
    private readonly Hitbox TopCollider;
    private readonly Hitbox RightCollider;
    private readonly Hitbox BottomCollider;
    Entity BackgroundRenderer;

    public ProceduralTilemap(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, new(0, 0, '0')) {
        Remove(Tiles);
        Remove(AnimatedTiles);
        Tag = 0;
        Depth = -10001;
        ID = id;
        TileWidth = Math.Max(1, data.Width / 8);
        TileHeight = Math.Max(1, data.Height / 8);
        MoveWithPlayer = data.Bool("MoveWithPlayer", false);
        XOffset = -TileWidth / 2;
        YOffset = -TileHeight / 2;
        if (MoveWithPlayer) {
            TileWidth = 40 * 4;
            TileHeight = 23 * 4;
            data.Width = TileWidth * 8;
            data.Height = TileHeight * 8;
            LeftCollider = new Hitbox(RegenerateMarginX + 100000000, data.Height, -100000000, 0) { Entity = this };
            TopCollider = new Hitbox(data.Width + 100000000, RegenerateMarginY + 100000000, 0, -100000000) { Entity = this };
            RightCollider = new Hitbox(RegenerateMarginX + 100000000, data.Height, data.Width - RegenerateMarginX, 0) { Entity = this };
            BottomCollider = new Hitbox(data.Width + 100000000, RegenerateMarginY, 0, data.Height - RegenerateMarginY) { Entity = this };
        }
        Arguments = data.String("Arguments", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        filePath = data.String("FilePath", "").Replace(".lua", "");
        Collidable = true;
    }


    private void OnLeft(Player player) => Scene.OnEndOfFrame += () => {
        int offset = 0;
        while (player.Collider.Collide(LeftCollider)) {
            offset++;
            X -= 8;
        }
        if (offset < 100) {
            for (int i = 0; i < offset; i++) {
                XOffset -= 1;
                Regenerate(player.Scene, RegenDir.Left);
            }
        } else {
            XOffset -= offset;
            GenerateAllTiles(player.Scene);
        }
    };

    private void OnTop(Player player) => Scene.OnEndOfFrame += () => {
        int offset = 0;
        while (player.Collider.Collide(TopCollider)) {
            offset++;
            Y -= 8;
        }
        if (offset < 100) {
            for (int i = 0; i < offset; i++) {
                YOffset -= 1;
                Regenerate(player.Scene, RegenDir.Up);
            }
        } else {
            YOffset -= offset;
            GenerateAllTiles(player.Scene);
        }
    };

    private void OnRight(Player player) => Scene.OnEndOfFrame += () => {
        int offset = 0;
        while (player.Collider.Collide(RightCollider)) {
            offset++;
            X += 8;
        }
        if (offset < 100) {
            for (int i = 0; i < offset; i++) {
                XOffset += 1;
                Regenerate(player.Scene, RegenDir.Right);
            }
        } else {
            XOffset += offset;
            GenerateAllTiles(player.Scene);
        }
    };

    private void OnBottom(Player player) => Scene.OnEndOfFrame += () => {
        int offset = 0;
        while (player.Collider.Collide(BottomCollider)) {
            offset++;
            Y += 8;
        }
        if (offset < 100) {
            for (int i = 0; i < offset; i++) {
                YOffset += 1;
                Regenerate(player.Scene, RegenDir.Down);
            }
        } else {
            YOffset += offset;
            GenerateAllTiles(player.Scene);
        }
    };

    public override void Added(Scene scene) {
        try
        {
            Lua lua = SandboxedLua.Instance;
            SandboxedLua.LoadFile(scene, filePath, $"ProceduralTilemap::{ID}");
            foreach (string arg in Arguments)
                lua.PushString(arg);
            if (lua.PCall(Arguments.Length, 1, 0) != LuaStatus.OK) {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"Failed to execute file {filePath}: {errorMessage ?? "<could not convert error message to string>"}");
            }
            if (!lua.IsTable(-1)) {
                lua.Pop(1);
                throw new LuaException($"Failed to execute file {filePath}: File must return a table.");
            }

            lua.GetField(-1, "before");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.beforeFuncRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(1);
                throw new LuaException($"Failed to execute file {filePath}: Returned field 'before' must be a function or nil.");
            }

            lua.GetField(-1, "foreground");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.foregroundFuncRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(1);
                throw new LuaException($"Failed to execute file {filePath}: Returned field 'foreground' must be a function or nil.");
            }

            lua.GetField(-1, "background");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.backgroundFuncRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(1);
                throw new LuaException($"Failed to execute file {filePath}: Returned field 'background' must be a function or nil.");
            }
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return;
        }

        GenerateAllTiles(scene);

        base.Added(scene);
    }

    VirtualMap<char> BGTiles;

    public bool GenerateAllTiles(Scene? scene = null) {
        scene ??= Scene;
        try {
            Lua lua = SandboxedLua.Instance;

            SandboxedLua.ActiveScene = scene;

            if (callbacks.beforeFuncRef is int beforeFunc) {
                int errorHandler = SandboxedLua.PushErrorHandler(lua);
                lua.RawGetInteger(LuaRegistry.Index, beforeFunc);
                lua.PushInteger(XOffset);
                lua.PushInteger(YOffset);
                lua.PushInteger(TileWidth);
                lua.PushInteger(TileHeight);
                if (lua.PCall(4, 0, errorHandler) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    throw new LuaException($"At {filePath} before({XOffset}, {YOffset}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
                }
            }

            BGTiles = new(TileWidth, TileHeight, '0');
            tileTypes = new(TileWidth, TileHeight, '0');
            for (int x = 0; x < TileWidth; x++) {
                for (int y = 0; y < TileHeight; y++) {
                    BGTiles[x, y] = '0';
                    tileTypes[x, y] = '0';
                    GenerateTile(lua, x + XOffset, y + YOffset, x, y);
                }
            }

            UseDeterministicAutotiling = true;
            TilingOffsetX = XOffset;
            TilingOffsetY = YOffset;
            var foreground = GFX.FGAutotiler.Generate(tileTypes, 0, 0, TileWidth, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
            var background = GFX.BGAutotiler.Generate(BGTiles, 0, 0, TileWidth, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
            UseDeterministicAutotiling = false;

            BackgroundRenderer ??= [];
            BackgroundRenderer.Position = Position;
            BGGrid?.RemoveSelf();
            BGAnim?.RemoveSelf();
            BackgroundRenderer.Add(BGAnim = background.SpriteOverlay);
            BackgroundRenderer.Add(BGGrid = background.TileGrid);
            BGGrid.ClipCamera = (scene as Level)!.Camera;
            BGAnim.ClipCamera = Tiles.ClipCamera;
            BackgroundRenderer.Depth = 9999;
            if (BackgroundRenderer.Scene != scene)
                scene.Add(BackgroundRenderer);

            Tiles?.RemoveSelf();
            AnimatedTiles?.RemoveSelf();
            Add(Tiles = foreground.TileGrid);
            Add(AnimatedTiles = foreground.SpriteOverlay);
            Collider = Grid = new Grid(TileWidth, TileHeight, 8f, 8f);
            for (int x = 0; x < tileTypes.Columns; x++)
                for (int y = 0; y < tileTypes.Rows; y++)
                    Grid[x, y] = tileTypes[x, y] != '0';

            return true;
        }
        catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return false;
        }
    }

    public override void Update() {
        if (Scene.Tracker.GetEntity<Player>() is Player player) {
            if (LeftCollider is not null && player.Collider.Collide(LeftCollider)) OnLeft(player);
            if (TopCollider is not null && player.Collider.Collide(TopCollider)) OnTop(player);
            if (RightCollider is not null && player.Collider.Collide(RightCollider)) OnRight(player);
            if (BottomCollider is not null && player.Collider.Collide(BottomCollider)) OnBottom(player);
        }
        LiftSpeed = Vector2.Zero;
        base.Update();
        LiftSpeed = Vector2.Zero;
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        if (BackgroundRenderer is not null) scene.Remove(BackgroundRenderer);
        BackgroundRenderer?.RemoveSelf();
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.HollowRect(Bounds, Color.Cyan);
        if (LeftCollider is not null) Draw.HollowRect(LeftCollider, Color.Pink);
        if (TopCollider is not null) Draw.HollowRect(TopCollider, Color.HotPink);
        if (RightCollider is not null) Draw.HollowRect(RightCollider, Color.LightPink);
        if (BottomCollider is not null) Draw.HollowRect(BottomCollider, Color.DeepPink);
    }

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
        Everest.Events.Level.OnExit += OnExit;
        IL.Celeste.Autotiler.Generate += ILAutotilerGenerate;
        On.Celeste.Player.Update += OnPlayerUpdate;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
        Everest.Events.Level.OnExit -= OnExit;
        IL.Celeste.Autotiler.Generate -= ILAutotilerGenerate;
        On.Celeste.Player.Update -= OnPlayerUpdate;
    }

    private static void OnReloadLevel(Level level) => WipeCache();
    private static void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) => WipeCache();

    private static void WipeCache() {
        Lua lua = SandboxedLua.Instance;

        foreach (LuaCallbacks callbacks in CallbackCache.Values) {
            if (callbacks.foregroundFuncRef is int fgFunc) lua.Unref(LuaRegistry.Index, fgFunc);
            if (callbacks.backgroundFuncRef is int bgFunc) lua.Unref(LuaRegistry.Index, bgFunc);
        }
        CallbackCache.Clear();
    }

    public override void MoveHExact(int move) { }

    public override void MoveVExact(int move) { }

    internal static bool UseDeterministicAutotiling = false;
    internal static int TilingOffsetX = 0;
    internal static int TilingOffsetY = 0;

    private static void ILAutotilerGenerate(ILContext il) {
        static Random SeedRandoIfDeterministic(Random rand, int k, int l) {
            if (!UseDeterministicAutotiling) return rand;
            return new Random((int)Utils.HashPosition(k + TilingOffsetX, l + TilingOffsetY));
        }

        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, static match => match.MatchLdsfld(typeof(Calc), nameof(Calc.Random)))) {
            Logger.Log(nameof(ScugHelperModule), "Found ILGenerate hook");
            cur.EmitLdloc(5); // k
            cur.EmitLdloc(7); // l
            cur.EmitDelegate(SeedRandoIfDeterministic);
        }
    }
    
    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        if (self.climbHopSolid is ProceduralTilemap) self.climbHopSolid = null;
        orig(self);
    }

    public enum RegenDir {
        Left, Up, Down, Right
    }

    public bool Regenerate(Scene scene, RegenDir dir) {
        scene ??= Scene;
        try
        {
            Lua lua = SandboxedLua.Instance;
            SandboxedLua.ActiveScene = scene;

            UseDeterministicAutotiling = true;
            TilingOffsetX = XOffset;
            TilingOffsetY = YOffset;

            switch (dir)
            {
                case RegenDir.Left:
                case RegenDir.Right: {
                    if (dir == RegenDir.Left)
                        for (int x = TileWidth - 1; x > 0; x--)
                            for (int y = 0; y < TileHeight; y++)
                            {
                                tileTypes[x, y] = tileTypes[x - 1, y];
                                Tiles.Tiles[x, y] = Tiles.Tiles[x - 1, y];
                                AnimatedTiles.tiles[x, y] = AnimatedTiles.tiles[x - 1, y];
                                BGTiles[x, y] = BGTiles[x - 1, y];
                                BGGrid.Tiles[x, y] = BGGrid.Tiles[x - 1, y];
                                BGAnim.tiles[x, y] = BGAnim.tiles[x - 1, y];
                            }
                    else
                        for (int x = 0; x < TileWidth - 1; x++)
                            for (int y = 0; y < TileHeight; y++)
                            {
                                tileTypes[x, y] = tileTypes[x + 1, y];
                                Tiles.Tiles[x, y] = Tiles.Tiles[x + 1, y];
                                AnimatedTiles.tiles[x, y] = AnimatedTiles.tiles[x + 1, y];
                                BGTiles[x, y] = BGTiles[x + 1, y];
                                BGGrid.Tiles[x, y] = BGGrid.Tiles[x + 1, y];
                                BGAnim.tiles[x, y] = BGAnim.tiles[x + 1, y];
                            }

                    int relativeX = dir == RegenDir.Left ? 0 : TileWidth - 1;
                    for (int y = 0; y < TileHeight; y++)
                        GenerateTile(lua, relativeX + XOffset, y + YOffset, relativeX, y);

                    var foreground = dir == RegenDir.Left
                        ? GFX.FGAutotiler.Generate(tileTypes, 0, 0, 3, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false })
                        : GFX.FGAutotiler.Generate(tileTypes, TileWidth - 4, 0, 3, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
                    var background = dir == RegenDir.Left
                        ? GFX.BGAutotiler.Generate(BGTiles, 0, 0, 3, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false })
                        : GFX.BGAutotiler.Generate(BGTiles, TileWidth - 4, 0, 3, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });

                    if (dir == RegenDir.Left)
                        for (int x = 0; x < 3; x++)
                            for (int y = 0; y < TileHeight; y++)
                            {
                                Tiles.Tiles[x, y] = foreground.TileGrid.Tiles[x, y];
                                AnimatedTiles.tiles[x, y] = foreground.SpriteOverlay.tiles[x, y];
                                BGGrid.Tiles[x, y] = background.TileGrid.Tiles[x, y];
                                BGAnim.tiles[x, y] = background.SpriteOverlay.tiles[x, y];
                            }
                    else
                        for (int x = 0; x < 3; x++)
                            for (int y = 0; y < TileHeight; y++)
                            {
                                Tiles.Tiles[x + TileWidth - 4, y] = foreground.TileGrid.Tiles[x, y];
                                AnimatedTiles.tiles[x + TileWidth - 4, y] = foreground.SpriteOverlay.tiles[x, y];
                                BGGrid.Tiles[x + TileWidth - 4, y] = background.TileGrid.Tiles[x, y];
                                BGAnim.tiles[x + TileWidth - 4, y] = background.SpriteOverlay.tiles[x, y];
                            }
                    break;
                }
                case RegenDir.Up:
                case RegenDir.Down: {
                        if (dir == RegenDir.Up)
                            for (int y = TileHeight - 1; y > 0; y--)
                                for (int x = 0; x < TileWidth; x++) {
                                    tileTypes[x, y] = tileTypes[x, y - 1];
                                    Tiles.Tiles[x, y] = Tiles.Tiles[x, y - 1];
                                    AnimatedTiles.tiles[x, y] = AnimatedTiles.tiles[x, y - 1];
                                    BGTiles[x, y] = BGTiles[x, y - 1];
                                    BGGrid.Tiles[x, y] = BGGrid.Tiles[x, y - 1];
                                    BGAnim.tiles[x, y] = BGAnim.tiles[x, y - 1];
                                }
                        else
                            for (int y = 0; y < TileHeight - 1; y++)
                                for (int x = 0; x < TileWidth; x++) {
                                    tileTypes[x, y] = tileTypes[x, y + 1];
                                    Tiles.Tiles[x, y] = Tiles.Tiles[x, y + 1];
                                    AnimatedTiles.tiles[x, y] = AnimatedTiles.tiles[x, y + 1];
                                    BGTiles[x, y] = BGTiles[x, y + 1];
                                    BGGrid.Tiles[x, y] = BGGrid.Tiles[x, y + 1];
                                    BGAnim.tiles[x, y] = BGAnim.tiles[x, y + 1];
                                }

                        int relativeY = dir == RegenDir.Up ? 0 : TileHeight - 1;
                        for (int x = 0; x < TileWidth; x++)
                            GenerateTile(lua, x + XOffset, relativeY + YOffset, x, relativeY);

                        var foreground = dir == RegenDir.Up
                            ? GFX.FGAutotiler.Generate(tileTypes, 0, 0, TileWidth, 3, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false })
                            : GFX.FGAutotiler.Generate(tileTypes, 0, TileHeight - 4, TileWidth, 3, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
                        var background = dir == RegenDir.Up
                            ? GFX.BGAutotiler.Generate(BGTiles, 0, 0, TileWidth, 3, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false })
                            : GFX.BGAutotiler.Generate(BGTiles, 0, TileHeight - 4, TileWidth, 3, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });

                        if (dir == RegenDir.Up)
                            for (int y = 0; y < 3; y++)
                                for (int x = 0; x < TileWidth; x++)
                                {
                                    Tiles.Tiles[x, y] = foreground.TileGrid.Tiles[x, y];
                                    AnimatedTiles.tiles[x, y] = foreground.SpriteOverlay.tiles[x, y];
                                    BGGrid.Tiles[x, y] = background.TileGrid.Tiles[x, y];
                                    BGAnim.tiles[x, y] = background.SpriteOverlay.tiles[x, y];
                                }
                        else
                            for (int y = 0; y < 3; y++)
                                for (int x = 0; x < TileWidth; x++)
                                {
                                    Tiles.Tiles[x, y + TileHeight - 4] = foreground.TileGrid.Tiles[x, y];
                                    AnimatedTiles.tiles[x, y + TileHeight - 4] = foreground.SpriteOverlay.tiles[x, y];
                                    BGGrid.Tiles[x, y + TileHeight - 4] = background.TileGrid.Tiles[x, y];
                                    BGAnim.tiles[x, y + TileHeight - 4] = background.SpriteOverlay.tiles[x, y];
                                }
                        break;
                    }
            }

            BackgroundRenderer.Position = Position;

            for (int x = 0; x < tileTypes.Columns; x++)
                for (int y = 0; y < tileTypes.Rows; y++)
                    Grid[x, y] = tileTypes[x, y] != '0';
            return true;
        }
        catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return false;
        } finally {
            UseDeterministicAutotiling = false;
        }
    }

    private void GenerateTile(Lua lua, int logicalX, int logicalY, int targetX, int targetY)
    {
        if (callbacks.foregroundFuncRef is int fgFunc)
        {
            int errorHandler = SandboxedLua.PushErrorHandler(lua);
            lua.RawGetInteger(LuaRegistry.Index, fgFunc);
            lua.PushInteger(logicalX);
            lua.PushInteger(logicalY);
            lua.PushInteger(TileWidth);
            lua.PushInteger(TileHeight);
            if (lua.PCall(4, 1, errorHandler) != LuaStatus.OK)
            {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"At {filePath} foreground({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
            }
            if (lua.IsNil(-1)) { tileTypes[targetX, targetY] = '0'; } else if (lua.IsString(-1)) { tileTypes[targetX, targetY] = lua.ToString(-1).First(); } else { throw new LuaException($"At {filePath} foreground({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): returned non-string non-nil value"); }
            lua.Pop(-1);
        }
        if (callbacks.backgroundFuncRef is int bgFunc)
        {
            int errorHandler = SandboxedLua.PushErrorHandler(lua);
            lua.RawGetInteger(LuaRegistry.Index, bgFunc);
            lua.PushInteger(logicalX);
            lua.PushInteger(logicalY);
            lua.PushInteger(TileWidth);
            lua.PushInteger(TileHeight);
            if (lua.PCall(4, 1, errorHandler) != LuaStatus.OK)
            {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"At {filePath} background({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
            }
            if (lua.IsNil(-1)) { BGTiles[targetX, targetY] = '0'; } else if (lua.IsString(-1)) { BGTiles[targetX, targetY] = lua.ToString(-1).First(); } else { throw new LuaException($"At {filePath} background({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): returned non-string non-nil value"); }
            lua.Pop(-1);
        }
    }
}
