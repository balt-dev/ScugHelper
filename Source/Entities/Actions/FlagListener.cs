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
[CustomEntity("ScugHelper/FlagListener")]
public class FlagListener(EntityData data, Vector2 _) : Entity(), IAction
{
    private readonly string[] Targets = IAction.GetTargets(data);
    private readonly bool State;
    private bool? stateLastTick;
    readonly string? flag = data.String("Flag");

    public void ActionUpdate(Level level) {
        var now = level.Session.GetFlag(flag);
        if (stateLastTick is bool state && (now == State) && !(state == State))
            ActionManager.AlertActions(Targets, level);
        stateLastTick = now;
    }
}
