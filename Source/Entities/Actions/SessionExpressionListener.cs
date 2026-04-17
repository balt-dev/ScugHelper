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
    private bool? stateLastTick;
    public SessionExpressionListener(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded)
            throw new Exception("FrostHelper must be loaded to use session expression actions.");
        Targets = IAction.GetTargets(data);
        Expression = data.String("Expression").Trim();
        if (!FrostHelperImports.TryCreateSessionExpression(Expression, out SessionExpression))
        {
            ActionManager.LogError($"FrostHelper session expression failed to compile: {Expression}");
            return;
        }
    }
    public void ActionUpdate(Level level)
    {
        var now = FrostHelperImports.GetBoolSessionExpressionValue(SessionExpression, level.Session);
        if (stateLastTick is bool state && now && !state)
            ActionManager.AlertActions(Targets, level);
        stateLastTick = now;
    }
}
