using KeraLua;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celeste.Mod.ScugHelper;

#nullable enable

public static class SandboxedLua {
    static int MainThreadID;

    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadID;

    internal static void LogLuaError(string message) {
        Logger.Error(nameof(ScugHelperModule), $"[LUA] {message}");
        Engine.Commands.Open = true;
        Engine.Commands.Log($"[LUA] Error: {message}", Color.Red);
    }

    static bool InitializedLua = false;
    internal static Scene? ActiveScene;
    static Lua LuaInstance = new(false);

    public static Lua Instance {
        get {
            //if (!IsMainThread) throw new InvalidOperationException("Cannot access the sandboxed Lua instance outside of the main thread.");
            return LuaInstance.MainThread;
        }
    }

    [Command("initlua", "Reinitializes the ScugHelper Lua instance.")]
    internal static void CmdInitLua() {
        LuaInstance = new(false);
        InitializedLua = false;
        InitLua();
    }
    
    [OnLoad]
    internal static void InitLua() {
        if (InitializedLua) return;
        InitializedLua = true;
        MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

        Instance.OpenLibs();

        Instance.PushCFunction(CustomRequire);
        Instance.SetGlobal("require");

        Instance.PushCFunction(CustomPrint);
        Instance.SetGlobal("print");

        Instance.DoString("""
            for _, needsNuke in ipairs { "os", "io", "debug", "package", "loadfile", "load", "loadstring", "dofile", "coroutine", "module", "collectgarbage", "newproxy", "getfenv", "setfenv", "rawget", "rawset" } do
                _G[needsNuke] = nil
            end

            math = setmetatable({}, { __metatable = false, __index = math, __newindex = function() error "cannot modify math module" end })
            string = setmetatable({}, { __metatable = false, __index = string, __newindex = function() error "cannot modify string module" end })
            table = setmetatable({}, { __metatable = false, __index = table, __newindex = function() error "cannot modify table module" end })
            utf8 = setmetatable({}, { __metatable = false, __index = utf8, __newindex = function() error "cannot modify utf8 module" end })

            _G = setmetatable({}, {
                __metatable = false,
                __index = _G,
                __newindex = function(t, key) error("cannot create or modify global variable " .. tostring(key) .. " - changing global state is disallowed, use locals only") end
            })
        """);
    }

    [Command("runlua", "Runs some lua in the ScugHelper sandboxed Lua instance.")]
    internal static void RunLua(string chunk) {
        if (chunk is null) return;
        Lua lua = Instance;
        Engine.Commands.Log(chunk);
        var res = lua.LoadString(chunk, "debugCommand");
        if (res is not LuaStatus.OK) {
            throw new LuaException($"failed to load chunk: {res}");
        }
        if (lua.PCall(0, 0, 0) != LuaStatus.OK) {
            string? errorMessage = lua.ToString(-1);
            throw new LuaException($"failed to execute: {errorMessage ?? "<could not convert error message to string>"}");
        }
    }

    internal static readonly Dictionary<string, int> RequireResults = [];

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
        Everest.Events.Level.OnExit += OnExit;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
        Everest.Events.Level.OnExit -= OnExit;
    }

    private static void OnReloadLevel(Level level) => WipeCache();
    private static void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) => WipeCache();

    private static void WipeCache() {
        Lua lua = Instance;

        foreach (int ret in RequireResults.Values) {
            lua.Unref(LuaRegistry.Index, ret);
        }
        RequireResults.Clear();
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
                LuaIntegration.InitFunctions();
            } else {
                if (ActivePaths.Contains(path))
                    lua.Error($"require recursion detected in {path}");
                ActivePaths.Add(path);
                if (!Everest.Content.TryGet(path, out ModAsset metadata, true))
                    lua.Error($"failed to read file: {path}");

                if (lua.LoadBuffer(metadata.Data, "luaRequire") != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Error($"failed to load file {path}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                if (lua.PCall(0, 1, 0) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Error($"failed to execute file {path}: {errorMessage ?? "<could not convert error message to string>"}");
                }
                Logger.Log(nameof(ScugHelperModule), $"Require returned type: {lua.Type(-1)}");
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

        Logger.Info(nameof(ScugHelperModule), $"[LUA] {string.Join('\t', strings)}");
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
}

public class LuaException(string msg) : Exception(msg) { }
