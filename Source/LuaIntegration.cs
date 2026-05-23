using System;
using System.Linq;
using KeraLua;
using Monocle;

namespace Celeste.Mod.ScugHelper;

internal static class LuaIntegration
{
    internal static void InitFunctions()
    {
        Lua lua = SandboxedLua.Instance;
        lua.NewTable();

        // scughelper.flags
        lua.NewTable();
        lua.NewTable();
        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(LuaGetFlag);
        lua.SetField(-2, "__index");
        lua.PushCFunction(ReadOnlyTable);
        lua.SetField(-2, "__newindex");
        lua.SetMetaTable(-2);
        lua.SetField(-2, "flags");

        // scughelper.counters
        lua.NewTable();
        lua.NewTable();
        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(LuaGetCounter);
        lua.SetField(-2, "__index");
        lua.PushCFunction(ReadOnlyTable);
        lua.SetField(-2, "__newindex");
        lua.SetMetaTable(-2);
        lua.SetField(-2, "counters");

        // scughelper.sliders
        lua.NewTable();
        lua.NewTable();
        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(LuaGetSlider);
        lua.SetField(-2, "__index");
        lua.PushCFunction(ReadOnlyTable);
        lua.SetField(-2, "__newindex");
        lua.SetMetaTable(-2);
        lua.SetField(-2, "sliders");

        lua.PushCFunction(LuaPerlinNoise);
        lua.SetField(-2, "perlin");

        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(ReadOnlyTable);
        lua.SetField(-2, "__newindex");
    }

    private static int LuaPerlinNoise(nint luaState)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        double x = lua.ToNumberX(1) ?? 0;
        double y = lua.ToNumberX(2) ?? 0;
        int octaves = (int) (lua.ToIntegerX(3) ?? 5);
        double persistence = lua.ToNumberX(4) ?? 0.5;
        int seed = (int) (lua.ToIntegerX(5) ?? 0);
        double res = Utils.Perlin.PerlinNoise(x, y, octaves, persistence, seed);
        lua.PushNumber(res);
        return 1;
    }

    private static int ReadOnlyTable(nint luaState)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        lua.Error($"cannot assign key {varName} to read-only table");
        return 0; // unreachable
    }

    private static int LuaGetFlag(nint luaState)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Error("could not convert to string");
        lua.PushBoolean(SandboxedLua.ActiveScene is Level level && (level.Session?.GetFlag(varName) ?? false));
        return 1;
    }

    private static int LuaGetCounter(nint luaState)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Error("could not convert to string");
        lua.PushInteger(SandboxedLua.ActiveScene is not Level level ? 0 : level.Session?.GetCounter(varName) ?? 0);
        return 1;
    }
    
    private static int LuaGetSlider(nint luaState)
    {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Error("could not convert to string");
        lua.PushNumber(SandboxedLua.ActiveScene is not Level level ? 0 : level.Session?.GetSlider(varName) ?? 0);
        return 1;
    }
}
