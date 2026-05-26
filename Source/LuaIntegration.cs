using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Celeste.Mod.ScugHelper.Entities.Actions;
using KeraLua;
using Monocle;

namespace Celeste.Mod.ScugHelper;

internal static class LuaIntegration
{
    internal static void InitFunctions() {
        Lua lua = SandboxedLua.Instance;
        lua.NewTable();

        // scughelper.flags
        lua.NewTable();
        lua.NewTable();
        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(LuaGetFlag);
        lua.SetField(-2, "__index");
        lua.PushCFunction(LuaSetFlag);
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
        lua.PushCFunction(LuaSetCounter);
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
        lua.PushCFunction(LuaSetSlider);
        lua.SetField(-2, "__newindex");
        lua.SetMetaTable(-2);
        lua.SetField(-2, "sliders");

        lua.PushCFunction(LuaPerlinNoise);
        lua.SetField(-2, "perlin");
        lua.PushCFunction(LuaAlertActions);
        lua.SetField(-2, "alertActions");

        lua.NewTable();
        lua.PushBoolean(false);
        lua.SetField(-2, "__metatable");
        lua.PushCFunction(ReadOnlyTable);
        lua.SetField(-2, "__newindex");
        lua.SetMetaTable(-2);
    }

    private static int LuaAlertActions(nint luaState)
    {
        if (SandboxedLua.ActiveScene is not Level level) return 0;
        Lua lua = Lua.FromIntPtr(luaState);
        if (!lua.IsTable(-1)) { lua.Error("attempted to alert action groups with non-table target set"); }
        
        int index = 1;
        while (true) {
            lua.PushInteger(index);
            lua.GetTable(-2);
            
            if (lua.IsNil(-1)) {
                lua.Pop(1);
                break;
            }
            
            string actionGroup = lua.ToString(-1) ?? "";
            try {
                ActionManager.AlertActions([actionGroup], level);
            } catch (Exception exc) {
                lua.Error($"unhandled exception while alerting actions: {exc}");
            }
            lua.Pop(1);
            index++;
        }
        
        lua.Pop(1);
        return 1;
    }

    private static int LuaPerlinNoise(nint luaState) {
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

    private static int ReadOnlyTable(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        lua.Err($"cannot assign key {varName} to read-only table");
        return 0; // unreachable
    }

    private static int LuaGetFlag(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        lua.PushBoolean(SandboxedLua.ActiveScene is Level level && (level.Session?.GetFlag(varName) ?? false));
        return 1;
    }
    
    private static int LuaSetFlag(nint luaState)
    {
        if (SandboxedLua.ActiveScene is not Level level) return 0;
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        bool value = lua.ToBoolean(3);
        level.Session.SetFlag(varName, value);
        return 1;
    }

    private static int LuaGetCounter(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        lua.PushInteger(SandboxedLua.ActiveScene is not Level level ? 0 : level.Session?.GetCounter(varName) ?? 0);
        return 1;
    }
    
    private static int LuaSetCounter(nint luaState)
    {
        if (SandboxedLua.ActiveScene is not Level level) return 0;
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        int? value = (int?) lua.ToIntegerX(3);
        if (value is not int val) { lua.Err("could not convert to integer"); throw new UnreachableException(); }
        level.Session.SetCounter(varName, val);
        return 1;
    }
    
    private static int LuaGetSlider(nint luaState) {
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        lua.PushNumber(SandboxedLua.ActiveScene is not Level level ? 0 : level.Session?.GetSlider(varName) ?? 0);
        return 1;
    }
    private static int LuaSetSlider(nint luaState)
    {
        if (SandboxedLua.ActiveScene is not Level level) return 0;
        Lua lua = Lua.FromIntPtr(luaState);
        string varName = lua.ToString(2);
        if (varName is null) lua.Err("could not convert to string");
        float? value = (float?) lua.ToNumberX(3);
        if (value is not float val) { lua.Err("could not convert to float"); throw new UnreachableException(); }
        level.Session.SetSlider(varName, val);
        return 1;
    }
}

internal static class LuaExt {
    [DoesNotReturn]
    public static void Err(this Lua self, string message) {
        self.Error(message);
    }
}