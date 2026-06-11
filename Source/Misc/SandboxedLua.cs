using KeraLua;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Celeste.Mod.ScugHelper;

#nullable enable

public static class SandboxedLua {
    static int MainThreadID;

    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadID;

    internal static void LogLuaError(string message) {
        Logger.Error(nameof(ScugHelper), $"[LUA] {message}");
        Engine.Commands.Open = true;
        Engine.Commands.Log($"[LUA] Error: {message}", Color.Red);
    }

    static bool InitializedLua = false;
    internal static Scene? ActiveScene;
    static Lua LuaInstance = new(false);

    public static Lua Instance {
        get {
            //if (!IsMainThread) throw new InvalidOperationException("Cannot access the sandboxed Lua instance outside of the main thread.");
            LuaStopwatch = Stopwatch.StartNew();
            return LuaInstance.MainThread;
        }
    }

    [Command("initlua", "Reinitializes the ScugHelper Lua instance.")]
    internal static void CmdInitLua() {
        LuaInstance = new(false);
        InitializedLua = false;
        InitLua();
        WipeCache();
    }

    [OnLoad]
    internal static void InitLua() {
        if (InitializedLua) return;
        InitializedLua = true;
        MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

        Instance.OpenLibs();

        Instance.SetHook(LuaHook, LuaHookMask.Count, 4096);

        Instance.PushCFunction(CustomRequire);
        Instance.SetGlobal("require");

        Instance.PushCFunction(CustomPrint);
        Instance.SetGlobal("print");

        LuaIntegration.InitFunctions();
        Instance.SetGlobal("scughelper");

        Instance.DoString("""
            os.setlocale("C")

            for _, needsNuke in ipairs { "os", "io", "debug", "package", "loadfile", "load", "loadstring", "dofile", "coroutine", "module", "collectgarbage", "newproxy", "getfenv", "setfenv", "rawget", "rawset", "pcall", "xpcall" } do
                _G[needsNuke] = nil
            end

            math = setmetatable({}, { __metatable = false, __index = math, __newindex = function() error "cannot modify math module" end })
            string = setmetatable({}, { __metatable = false, __index = string, __newindex = function() error "cannot modify string module" end })
            table = setmetatable({}, { __metatable = false, __index = table, __newindex = function() error "cannot modify table module" end })
            utf8 = setmetatable({}, { __metatable = false, __index = utf8, __newindex = function() error "cannot modify utf8 module" end })

            setmetatable(_G, {
                __metatable = false,
                __newindex = function(t, key) error("cannot create or modify global variable " .. tostring(key) .. " - changing global state is disallowed, use locals only") end
            })
        """);

        nint ud = 0;
        var luaAlloc = Instance.GetAllocFunction(ref ud);
        Instance.SetAllocFunction(Alloc, ref ud);
    }

    static long AllocatedLuaMemory = 0;

    static nint Alloc(nint ud, nint ptr, nuint osize, nuint nsize) {
        AllocatedLuaMemory = Math.Max(0, AllocatedLuaMemory);
        try {
            if (AllocatedLuaMemory + (long) (nsize - osize) > ((long) ScugHelperModule.Settings.LuaMemoryLimit) * 1024 * 1024)
                throw new OutOfMemoryException();
            unsafe {
                if (nsize == 0) {
                    AllocatedLuaMemory -= (long) osize;
                    NativeMemory.Free((void*)ptr);
                    return 0;
                }
                else {
                    AllocatedLuaMemory += (long) (nsize - osize);
                    return (nint)NativeMemory.Realloc((void*)ptr, nsize);
                }
            }
        } catch (OutOfMemoryException) {
            AllocatedLuaMemory = 0;
            Instance.Error("out of memory - increase the memory limit in the mod settings if need be");
            return 0; // unreachable
        }
    }

    static Stopwatch LuaStopwatch = Stopwatch.StartNew();

    private static void LuaHook(nint luaState, nint ar)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        if (LuaStopwatch.Elapsed.Ticks > ScugHelperModule.Settings.LuaTimeLimit * TimeSpan.TicksPerSecond) {
            lua.Error("execution time limit reached - increase the time limit in the mod settings if need be");
        }
    }

    [Command("runlua", "Runs some lua in the ScugHelper sandboxed Lua instance.")]
    internal static void RunLua() {
        string chunk = Engine.Commands.commandHistory[0][6..];

        Lua lua = Instance;
        lua.SetTop(0);
        int errorHandler = PushErrorHandler(lua);
        LoadInlineString(Engine.Scene, chunk, "debugCommand");
        if (lua.PCall(0, 1, errorHandler) != LuaStatus.OK) {
            string? errorMessage = lua.ToString(-1);
            throw new LuaException($"failed to execute: {errorMessage ?? "<could not convert error message to string>"}");
        }
        if (lua.PCall(0, 0, errorHandler) != LuaStatus.OK) {
            string? errorMessage = lua.ToString(-1);
            throw new LuaException($"failed to execute: {errorMessage ?? "<could not convert error message to string>"}");
        }
        lua.Pop(1);
    }

    private static readonly Dictionary<string, int> ChunkCache = [];

    /// Loads a file and pushes its chunk onto the Lua stack. Will throw a LuaException if the file could not be read.
    public static void LoadFile(Scene scene, string path, string chunkName) {
        Lua lua = Instance;
        ActiveScene = scene;

        if (!ChunkCache.TryGetValue(path, out int chunk)) {
            if (!Everest.Content.TryGet(path, out ModAsset metadata, true))
                throw new LuaException($"Failed to read file: {path}");

            if (lua.LoadBuffer(metadata.Data, chunkName) != LuaStatus.OK) {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"Failed to load file {path}: {errorMessage ?? "<could not convert error message to string>"}");
            }
            ChunkCache.Add(path, chunk = lua.Ref(LuaRegistry.Index));
        }
        lua.RawGetInteger(LuaRegistry.Index, chunk);
    }

    public static void LoadInlineString(Scene scene, string chunk, string chunkName) {
        Lua lua = Instance;
        ActiveScene = scene;
        if (lua.LoadString($"return function() {chunk} end", chunkName) != LuaStatus.OK) {
            string? errorMessage = lua.ToString(-1);
            throw new LuaException($"Failed to load chunk {chunkName}: {errorMessage ?? "<could not convert error message to string>"}");
        }
    }

    internal static readonly Dictionary<string, int> RequireResults = [];

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
        Everest.Events.Level.OnExit += OnExit;
        On.Celeste.Player.Update += OnPlayerUpdate;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
        Everest.Events.Level.OnExit -= OnExit;
        On.Celeste.Player.Update -= OnPlayerUpdate;
    }

    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        ActiveScene = self.level;
        orig(self);
    }

    private static void OnReloadLevel(Level level) => WipeCache(level);
    private static void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) => WipeCache();

    private static void WipeCache(Level? level = null) {
        ActiveScene = level;

        Lua lua = Instance;

        foreach (int ret in RequireResults.Values) lua.Unref(LuaRegistry.Index, ret);
        foreach (int ret in ChunkCache.Values) lua.Unref(LuaRegistry.Index, ret);

        RequireResults.Clear();
        ChunkCache.Clear();
    }

    static readonly HashSet<string> ActivePaths = [];

    private static int CustomRequire(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        string path = lua.ToString(1).Replace(".", "/");
        lua.Pop(-1);
        if (RequireResults.TryGetValue(path, out int res))
            lua.RawGetInteger(LuaRegistry.Index, res);
        else {
            if (path == "scughelper") {
                lua.GetGlobal("scughelper");
            } else {
                if (ActivePaths.Contains(path))
                    lua.Err($"require recursion detected in {path}");
                ActivePaths.Add(path);
                if (!Everest.Content.TryGet(path, out ModAsset metadata, true))
                    lua.Err($"failed to read file: {path}");

                int errorHandler = PushErrorHandler(lua);
                if (lua.LoadBuffer(metadata.Data, "luaRequire") != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Err($"failed to load file {path}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                if (lua.PCall(0, 1, errorHandler) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Err($"failed to execute file {path}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                Logger.Log(nameof(ScugHelper), $"Require returned type: {lua.Type(-1)}");
            }
            lua.PushCopy(-1);
            RequireResults.Add(path, lua.Ref(LuaRegistry.Index));
            ActivePaths.Remove(path);
        }

        return 1;
    }

    private static int CustomPrint(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        int argCount = lua.GetTop();
        List<string> strings = [];
        for (int i = 1; i <= argCount; i++) {
            lua.PushCopy(i);
            strings.Add(LuaValueToString(lua));
        }

        Logger.Info(nameof(ScugHelper), $"[LUA] {string.Join('\t', strings)}");
        Engine.Commands.Open = true;
        Engine.Commands.Log($"[LUA] {string.Join('\t', strings).Replace("\t", "    ")}", Color.White);

        return 0;
    }

    private const int DepthLimit = 3;

    public static string LuaValueToString(Lua lua) => LuaValueToString(lua, 0);

    private static string LuaValueToString(Lua lua, int depth) {
        if (!lua.IsTable(-1)) {
            string res = lua.ToString(-1) ?? $"[{lua.TypeName(lua.Type(-1))}]";;
            lua.Pop(1);
            return res;
        }
        if (depth > DepthLimit) return "...";

        StringBuilder builder = new();
        builder.Append("{ ");
        lua.PushNil();
        bool first = true;
        while (lua.Next(-2)) {
            if (!first) builder.Append(", ");
            first = false;
            lua.PushCopy(-2);
            string key = lua.ToString(-1) ?? $"[{lua.TypeName(lua.Type(-1))}]";
            lua.Pop(1);
            builder.Append($"{key} = ");
            string value = LuaValueToString(lua, depth + 1);
            builder.Append(value);
        }
        lua.Pop(1);
        builder.Append(" }");

        return builder.ToString();
    }

    internal static int ErrorHandler(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        lua.Traceback(lua, 0);
        string traceback = lua.ToString(-1);
        lua.Pop(1);
        string message = lua.ToString(-1);
        lua.SetTop(0);
        lua.PushString(message + "\n" + traceback);
        return 1;
    }

    internal static int PushErrorHandler(Lua lua)
    {
        lua.PushCFunction(ErrorHandler);
        return lua.GetTop();
    }
}

public class LuaException(string msg) : Exception(msg) { }
