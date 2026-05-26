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
[CustomEntity("ScugHelper/SandboxedLuaTrigger")]
public class SandboxedLuaTrigger(EntityData data, Vector2 offset, EntityID id) : Trigger(data, offset)
{
    readonly string[] Arguments = data.String("Arguments", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    readonly string filePath = data.String("FilePath", "").Replace(".lua", "");
    LuaCallbacks callbacks;

    struct LuaCallbacks {
        internal int? onEnterRef;
        internal int? onStayRef;
        internal int? onLeaveRef;
    }

    readonly EntityID ID = id;

    public override void Awake(Scene scene) {
        try
        {
            Lua lua = SandboxedLua.Instance;
            lua.SetTop(0);
            int errorHandler = SandboxedLua.PushErrorHandler(lua);
            SandboxedLua.LoadFile(scene, filePath, $"SandboxedLuaTrigger::{ID}");
            foreach (string arg in Arguments)
                lua.PushString(arg);
            if (lua.PCall(Arguments.Length, 1, errorHandler) != LuaStatus.OK) {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"In {ID}: Failed to execute file {filePath}: {errorMessage ?? "<could not convert error message to string>"}");
            }
            if (!lua.IsTable(-1)) {
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {filePath}: File must return a table.");
            }
            
            lua.GetField(-1, "onEnter");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.onEnterRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {filePath}: Returned field 'onEnter' must be a function or nil.");
            }

            lua.GetField(-1, "onStay");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.onStayRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {filePath}: Returned field 'onStay' must be a function or nil.");
            }

            lua.GetField(-1, "onLeave");
            if (lua.IsNil(-1)) lua.Pop(1);
            else if (lua.IsFunction(-1)) callbacks.onLeaveRef = lua.Ref(LuaRegistry.Index);
            else {
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {filePath}: Returned field 'onLeave' must be a function or nil.");
            }
            lua.SetTop(0);
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return;
        }

        base.Awake(scene);
    }
    
    public override void OnEnter(Player player) {
        base.OnEnter(player);
        try {
            if (callbacks.onEnterRef is int func) {
                Lua lua = SandboxedLua.Instance;
                lua.SetTop(0);
                int errorHandler = SandboxedLua.PushErrorHandler(lua);
                lua.RawGetInteger(LuaRegistry.Index, func);
                lua.PushNumber(X);
                lua.PushNumber(Y);
                lua.PushNumber(Width);
                lua.PushNumber(Height);
                if (lua.PCall(4, 0, errorHandler) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Pop(2);
                    throw new LuaException($"In {ID}: Failed to execute file {filePath} onEnter({X}, {Y}, {Width}, {Height}): {errorMessage ?? "<could not convert error message to string>"}");
                }
                lua.Pop(1);
            }
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            return;
        }
    }
    
    public override void OnStay(Player player) {
        base.OnStay(player);
        try {
            if (callbacks.onStayRef is int func) {
                Lua lua = SandboxedLua.Instance;
                lua.SetTop(0);
                int errorHandler = SandboxedLua.PushErrorHandler(lua);
                lua.RawGetInteger(LuaRegistry.Index, func);
                lua.PushNumber(X);
                lua.PushNumber(Y);
                lua.PushNumber(Width);
                lua.PushNumber(Height);
                if (lua.PCall(4, 0, errorHandler) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Pop(2);
                    throw new LuaException($"In {ID}: Failed to execute file {filePath} onStay({X}, {Y}, {Width}, {Height}): {errorMessage ?? "<could not convert error message to string>"}");
                }
                lua.Pop(1);
            }
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            return;
        }
    }
    
    public override void OnLeave(Player player) {
        try {
            base.OnLeave(player);
            if (callbacks.onLeaveRef is int func) {
                Lua lua = SandboxedLua.Instance;
                lua.SetTop(0);
                int errorHandler = SandboxedLua.PushErrorHandler(lua);
                lua.RawGetInteger(LuaRegistry.Index, func);
                lua.PushNumber(X);
                lua.PushNumber(Y);
                lua.PushNumber(Width);
                lua.PushNumber(Height);
                if (lua.PCall(4, 0, errorHandler) != LuaStatus.OK) {
                    string? errorMessage = lua.ToString(-1);
                    lua.Pop(2);
                    throw new LuaException($"In {ID}: Failed to execute file {filePath} onLeave({X}, {Y}, {Width}, {Height}): {errorMessage ?? "<could not convert error message to string>"}");
                }
                lua.Pop(1);
            }
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            return;
        }
    }
}
