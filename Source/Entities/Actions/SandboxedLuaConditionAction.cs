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

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/SandboxedLuaConditionAction")]
public class SandboxedLuaConditionAction(EntityData data, Vector2 _, EntityID id) : Entity(), IAction
{
    readonly string[] Arguments = data.String("Arguments", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    readonly string filePath = data.String("FilePath", "").Replace(".lua", "");
    private readonly string[] Targets = IAction.GetTargets(data);
    private readonly bool Invert;
    readonly string? inlineLua = data.String("InlineLua");
    
    readonly EntityID ID = id;

    int? Callback;

    public override void Awake(Scene scene) {
        try
        {
            Lua lua = SandboxedLua.Instance;
            lua.SetTop(0);
            int errorHandler = SandboxedLua.PushErrorHandler(lua);
            if (inlineLua is not null)
                SandboxedLua.LoadInlineString(scene, inlineLua, $"SandboxedLuaConditionAction::{ID}.inline");
            else
                SandboxedLua.LoadFile(scene, filePath, $"SandboxedLuaConditionAction::{ID}");
            foreach (string arg in Arguments)
                lua.PushString(arg);
            if (lua.PCall(Arguments.Length, 1, errorHandler) != LuaStatus.OK) {
                string? errorMessage = lua.ToString(-1);
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {(inlineLua is null ? filePath : "<inline chunk>")}: {errorMessage ?? "<could not convert error message to string>"}");
            }
            if (!lua.IsFunction(-1)) {
                lua.Pop(2);
                throw new LuaException($"In {ID}: Failed to execute file {(inlineLua is null ? filePath : "<inline chunk>")}: File must return a function.");
            }
            Callback = lua.Ref(LuaRegistry.Index);
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            RemoveSelf();
            return;
        }

        base.Awake(scene);
    }
    
    public void Alert(Level level) {
        try {
            if (Callback is not int cb) throw new NullReferenceException("Callback is null????");
            SandboxedLua.ActiveScene = level;
            Lua lua = SandboxedLua.Instance;
            lua.SetTop(0);
            int errorHandler = SandboxedLua.PushErrorHandler(lua);
            lua.RawGetInteger(LuaRegistry.Index, cb);
            if (lua.PCall(0, 1, errorHandler) != LuaStatus.OK) {
                string? errorMessage = lua.ToString(-1);
                throw new LuaException($"In {ID}: At {(inlineLua is null ? filePath : "<inline chunk>")}: " + (errorMessage ?? "<could not convert error message to string>"));
            }
            var res = lua.ToBoolean(-1);
            lua.Pop(1);
            if (res ^ Invert)
                ActionManager.AlertActions(Targets, level);
        } catch (Exception err) {
            SandboxedLua.ActiveScene = null;
            SandboxedLua.LogLuaError(err.ToString());
            return;
        }
    }
    public override void Removed(Scene scene) {
        if (Callback is int cb) SandboxedLua.Instance.Unref(LuaRegistry.Index, cb);
    }
}
