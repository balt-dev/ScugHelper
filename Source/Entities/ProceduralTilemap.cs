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

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/ProceduralTilemap")]
public class ProceduralTilemap : Platform
{

    private readonly EntityID ID;
    public readonly int TileWidth;
    public readonly int TileHeight;
    public SolidTiles FGSolid { get; internal set; }
    public TileGrid BGGrid { get; internal set; }
    public TileGrid FGGrid { get; internal set; }
    public AnimatedTiles BGAnim { get; internal set; }
    public AnimatedTiles FGAnim { get; internal set; }
    public readonly bool AbsoluteX;
    public readonly bool AbsoluteY;
    public readonly bool MoveWithPlayer;
    public readonly float RegenerateMarginX;
    public readonly float RegenerateMarginY;
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

    public ProceduralTilemap(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, true)
    {
        Depth = -10001;
        ID = id;
        TileWidth = Math.Max(1, data.Width / 8);
        TileHeight = Math.Max(1, data.Height / 8);
        MoveWithPlayer = data.Bool("MoveWithPlayer", false);
        if (MoveWithPlayer) {
            TileWidth = 40 * 4;
            TileHeight = 23 * 4;
            RegenerateMarginX = 40 * 8;
            RegenerateMarginY = 23 * 8;
            Add(new PlayerCollider(OnLeft, LeftCollider = new Hitbox(RegenerateMarginX + 100000000, data.Height, -100000000, 0) { Entity = this }));
            Add(new PlayerCollider(OnTop, TopCollider = new Hitbox(data.Width + 100000000, RegenerateMarginY + 100000000, 0, -100000000) { Entity = this }));
            Add(new PlayerCollider(OnRight, RightCollider = new Hitbox(RegenerateMarginX + 100000000, data.Height, data.Width - RegenerateMarginX, 0) { Entity = this }));
            Add(new PlayerCollider(OnBottom, BottomCollider = new Hitbox(data.Width + 100000000, RegenerateMarginY, 0, data.Height - RegenerateMarginY) { Entity = this }));
        }
        XOffset = -TileWidth / 2;
        YOffset = -TileHeight / 2;
        Arguments = data.String("Arguments", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        filePath = data.String("FilePath", "").Replace(".lua", "");
    }


    private void OnLeft(Player player) => Scene.OnEndOfFrame += () => {
        while (true)
        {
            Collider = LeftCollider;
            if (!player.CollideCheck(this)) break;
            X -= RegenerateMarginX;
            XOffset -= (int)RegenerateMarginX / 8;
        }
        RegenerateTiles(player.Scene);
    };

    private void OnTop(Player player) => Scene.OnEndOfFrame += () => {
        while (true)
        {
            Collider = TopCollider;
            if (!player.CollideCheck(this)) break;
            Y -= RegenerateMarginY;
            YOffset -= (int)RegenerateMarginY / 8;
        }
        RegenerateTiles(player.Scene);
    };

    private void OnRight(Player player) => Scene.OnEndOfFrame += () => {
        while (true)
        {
            Collider = RightCollider;
            if (!player.CollideCheck(this)) break;
            X += RegenerateMarginX;
            XOffset += (int)RegenerateMarginX / 8;
        }
        RegenerateTiles(player.Scene);
    };

    private void OnBottom(Player player) => Scene.OnEndOfFrame += () => {
        while (true)
        {
            Collider = BottomCollider;
            if (!player.CollideCheck(this)) break;
            Y += RegenerateMarginY;
            YOffset += (int)RegenerateMarginY / 8;
        }
        RegenerateTiles(player.Scene);
    };

    public override void Awake(Scene scene) {
        try
        {
            Lua lua = SandboxedLua.Instance;
            SandboxedLua.ActiveScene = scene;

            if (!CallbackCache.TryGetValue(ID, out callbacks))
            {

                // We do this here so it's on the main thread
                if (!Everest.Content.TryGet(filePath, out ModAsset metadata, true))
                    throw new LuaException($"Failed to read file: {filePath}");

                if (lua.LoadBuffer(metadata.Data, "proceduralTilemap") != LuaStatus.OK)
                {
                    string? errorMessage = lua.ToString(-1);
                    throw new LuaException($"Failed to load file {filePath}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                foreach (string arg in Arguments)
                    lua.PushString(arg);
                if (lua.PCall(Arguments.Length, 1, 0) != LuaStatus.OK)
                {
                    string? errorMessage = lua.ToString(-1);
                    throw new LuaException($"Failed to execute file {filePath}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                if (!lua.IsTable(-1))
                {
                    lua.Pop(1);
                    throw new LuaException($"Failed to execute file {filePath}: File must return a table.");
                }
                
                lua.GetField(-1, "before");
                if (lua.IsNil(-1)) lua.Pop(1);
                else if (lua.IsFunction(-1)) callbacks.beforeFuncRef = lua.Ref(LuaRegistry.Index);
                else
                {
                    lua.Pop(1);
                    throw new LuaException($"Failed to execute file {filePath}: Returned field 'before' must be a function or nil.");
                }

                lua.GetField(-1, "foreground");
                if (lua.IsNil(-1)) lua.Pop(1);
                else if (lua.IsFunction(-1)) callbacks.foregroundFuncRef = lua.Ref(LuaRegistry.Index);
                else
                {
                    lua.Pop(1);
                    throw new LuaException($"Failed to execute file {filePath}: Returned field 'foreground' must be a function or nil.");
                }

                lua.GetField(-1, "background");
                if (lua.IsNil(-1)) lua.Pop(1);
                else if (lua.IsFunction(-1)) callbacks.backgroundFuncRef = lua.Ref(LuaRegistry.Index);
                else
                {
                    lua.Pop(1);
                    throw new LuaException($"Failed to execute file {filePath}: Returned field 'background' must be a function or nil.");
                }

                CallbackCache.Add(ID, callbacks);
            }
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return;
        }

        RegenerateTiles(scene);

        base.Awake(scene);
    }
    public bool RegenerateTiles(Scene? scene = null)
    {
        scene ??= Scene;
        try
        {
            Lua lua = SandboxedLua.Instance;

            SandboxedLua.ActiveScene = scene;
            
            if (callbacks.beforeFuncRef is int beforeFunc)
            {
                lua.RawGetInteger(LuaRegistry.Index, beforeFunc);
                lua.PushInteger(XOffset);
                lua.PushInteger(YOffset);
                lua.PushInteger(TileWidth);
                lua.PushInteger(TileHeight);
                if (lua.PCall(4, 0, 0) != LuaStatus.OK)
                {
                    string? errorMessage = lua.ToString(-1);
                    throw new LuaException($"At {filePath} before({XOffset}, {YOffset}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
                }
            }
            
            VirtualMap<char> bgTiles = new(TileWidth, TileHeight, '0');
            VirtualMap<char> fgTiles = new(TileWidth, TileHeight, '0');
            for (int x = 0; x < TileWidth; x++)
            {
                for (int y = 0; y < TileHeight; y++)
                {
                    bgTiles[x, y] = '0';
                    fgTiles[x, y] = '0';
                    int logicalX = x + XOffset;
                    int logicalY = y + YOffset;
                    if (callbacks.foregroundFuncRef is int fgFunc)
                    {
                        lua.RawGetInteger(LuaRegistry.Index, fgFunc);
                        lua.PushInteger(logicalX);
                        lua.PushInteger(logicalY);
                        lua.PushInteger(TileWidth);
                        lua.PushInteger(TileHeight);
                        if (lua.PCall(4, 1, 0) != LuaStatus.OK)
                        {
                            string? errorMessage = lua.ToString(-1);
                            throw new LuaException($"At {filePath} foreground({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
                        }
                        if (lua.IsNil(-1)) { fgTiles[x, y] = '0'; }
                        else if (lua.IsString(-1)) { fgTiles[x, y] = lua.ToString(-1).First(); }
                        else { throw new LuaException($"At {filePath} foreground({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): returned non-string non-nil value"); }
                        lua.Pop(-1);
                    }
                    if (callbacks.backgroundFuncRef is int bgFunc)
                    {
                        lua.RawGetInteger(LuaRegistry.Index, bgFunc);
                        lua.PushInteger(logicalX);
                        lua.PushInteger(logicalY);
                        lua.PushInteger(TileWidth);
                        lua.PushInteger(TileHeight);
                        if (lua.PCall(4, 1, 0) != LuaStatus.OK)
                        {
                            string? errorMessage = lua.ToString(-1);
                            throw new LuaException($"At {filePath} background({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): " + (errorMessage ?? "<could not convert to string>"));
                        }
                        if (lua.IsNil(-1)) { bgTiles[x, y] = '0'; }
                        else if (lua.IsString(-1)) { bgTiles[x, y] = lua.ToString(-1).First(); }
                        else { throw new LuaException($"At {filePath} background({logicalX}, {logicalY}, {TileWidth}, {TileHeight}): returned non-string non-nil value"); }
                        lua.Pop(-1);
                    }
                }
            }

            UseDeterministicAutotiling = true;
            TilingOffsetX = (int)(X / 8);
            TilingOffsetY = (int)(Y / 8);
            var foreground = GFX.FGAutotiler.Generate(fgTiles, 0, 0, TileWidth, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
            var background = GFX.BGAutotiler.Generate(bgTiles, 0, 0, TileWidth, TileHeight, false, '0', new Autotiler.Behaviour { EdgesExtend = true, EdgesIgnoreOutOfLevel = false, PaddingIgnoreOutOfLevel = false });
            UseDeterministicAutotiling = false;

            var oldBackgroundRender = BackgroundRenderer;

            BackgroundRenderer ??= [];
            BackgroundRenderer.Position = Position;
            BGGrid?.RemoveSelf();
            BGAnim?.RemoveSelf();
            BackgroundRenderer.Add(BGAnim = background.SpriteOverlay);
            BackgroundRenderer.Add(BGGrid = background.TileGrid);
            BackgroundRenderer.Depth = 9999;
            Scene.Add(BackgroundRenderer);

            FGGrid?.RemoveSelf();
            FGAnim?.RemoveSelf();
            FGSolid?.RemoveSelf();
            Add(FGGrid = foreground.TileGrid);
            Add(FGAnim = foreground.SpriteOverlay);
            Scene.Add(FGSolid = new(Position, fgTiles));

            return true;
        }
        catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return false;
        }
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        FGSolid?.RemoveSelf();
        BackgroundRenderer?.RemoveSelf();
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.HollowRect(Bounds, Color.Cyan);
    }

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
        Everest.Events.Level.OnExit += OnExit;
        IL.Celeste.Autotiler.Generate += ILGenerate;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
        Everest.Events.Level.OnExit -= OnExit;
        IL.Celeste.Autotiler.Generate -= ILGenerate;
    }

    private static void OnReloadLevel(Level level) => WipeCache();
    private static void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) => WipeCache();

    private static void WipeCache()
    {
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



    private static void ILGenerate(ILContext il)
    {
        static Random SeedRandoIfDeterministic(Random rand, int k, int l) {
            if (!UseDeterministicAutotiling) return rand;
            return new Random((int)Utils.HashPosition(k + TilingOffsetX, l + TilingOffsetY));
        }

        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, static match => match.MatchLdsfld(typeof(Calc), nameof(Calc.Random))))
        {
            Logger.Log(nameof(ScugHelperModule), "Found ILGenerate hook");
            cur.EmitLdloc(5); // k
            cur.EmitLdloc(7); // l
            cur.EmitDelegate(SeedRandoIfDeterministic);
        }
    }
}
