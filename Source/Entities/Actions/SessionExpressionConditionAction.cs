using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SessionExpressionConditionAction")]
public class SessionExpressionConditionAction : Entity, IAction
{
    private readonly object? SessionExpression;
    private readonly string[] Targets = [];
    private readonly string Expression = "";
    private readonly bool Invert;
    public SessionExpressionConditionAction(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded) {
            ActionManager.LogError($"FrostHelper is not loaded! Session Expression actions won't work.");
            return;
        }
        Targets = IAction.GetTargets(data);
        Invert = data.Bool("Invert");
        Expression = data.String("Expression", "").Trim();
        if (!FrostHelperImports.TryCreateSessionExpression(Expression, out SessionExpression!)) {
            ActionManager.LogError($"FrostHelper session expression failed to compile: {Expression}");
            return;
        }
    }
    public void Alert(Level level) {
        if (!FrostHelperImports.IsLoaded) return;
        if (SessionExpression is null) return;
        if (FrostHelperImports.GetBoolSessionExpressionValue(SessionExpression, level.Session) ^ Invert)
            ActionManager.AlertActions(Targets, level);
    }
}
