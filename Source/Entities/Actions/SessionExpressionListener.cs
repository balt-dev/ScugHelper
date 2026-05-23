using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SessionExpressionListener")]
public class SessionExpressionListener : Entity, IAction
{
    private readonly object? SessionExpression;
    public readonly string[] Targets;
    private readonly string Expression;
    private readonly bool Invert;
    private bool? stateLastTick;
    public SessionExpressionListener(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded) {
            ActionManager.LogError($"FrostHelper is not loaded! Session Expression actions won't work.");
            return;
        }
        Targets = IAction.GetTargets(data);
        Expression = data.String("Expression").Trim();
        Invert = data.Bool("Invert");
        if (!FrostHelperImports.TryCreateSessionExpression(Expression, out SessionExpression)) {
            ActionManager.LogError($"FrostHelper session expression failed to compile: {Expression}");
            return;
        }
    }
    public void ActionUpdate(Level level) {
        if (!FrostHelperImports.IsLoaded) return;
        if (SessionExpression is null) return;
        var now = FrostHelperImports.GetBoolSessionExpressionValue(SessionExpression, level.Session);
        if (stateLastTick is bool state && (now ^ Invert) && !(state ^ Invert))
            ActionManager.AlertActions(Targets, level);
        stateLastTick = now;
    }
}
