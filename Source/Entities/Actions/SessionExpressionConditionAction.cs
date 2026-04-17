using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SessionExpressionConditionAction")]
public class SessionExpressionConditionAction : Entity, IAction
{
    private readonly object SessionExpression;
    private readonly string[] Targets;
    private readonly string Expression;
    public SessionExpressionConditionAction(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded)
            throw new Exception("FrostHelper must be loaded to use session expression actions.");
        Targets = IAction.GetTargets(data);
        Expression = data.String("Expression").Trim();
        if (!FrostHelperImports.TryCreateSessionExpression(Expression, out SessionExpression!))
        {
            ActionManager.LogError($"FrostHelper session expression failed to compile: {Expression}");
            return;
        }
    }
    public void Alert(Level level)
    {
        if (FrostHelperImports.GetBoolSessionExpressionValue(SessionExpression, level.Session))
            ActionManager.AlertActions(Targets, level);
    }
}
