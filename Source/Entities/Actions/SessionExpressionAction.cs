using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SessionExpressionAction")]
public class SessionExpressionAction : Entity, IAction
{
    internal enum SessionValueKind { Flag, Counter, Slider };

    private readonly object SessionExpression;
    private readonly string Target;
    private readonly string Expression;
    public SessionExpressionAction(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded) {
            ActionManager.LogError($"FrostHelper is not loaded! Session Expression actions won't work.");
            return;
        }
        Target = data.String("Target").Trim();
        Expression = data.String("Expression").Trim();
        if (!FrostHelperImports.TryCreateSessionExpression(Expression, out SessionExpression!)) {
            ActionManager.LogError($"FrostHelper session expression failed to compile: {Expression}");
            return;
        }
    }
    public void Alert(Level level) {
        if (!FrostHelperImports.IsLoaded) return;
        if (SessionExpression is null) return;
        Player? player = level.Tracker.GetEntity<Player>();
        switch (Target) {
            case "$player.x": player.X = player.PreviousPosition.X = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session); return;
            case "$player.y": player.Y = player.PreviousPosition.Y = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session); return;
            case "$subpixel.x": player.movementCounter.X = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session) % 1.0f; return;
            case "$subpixel.y": player.movementCounter.Y = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session) % 1.0f; return;
            case "$speed.x": player.Speed.X = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session); return;
            case "$speed.y": player.Speed.Y = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session); return;
            case "$stamina": player.Stamina = FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session); return;
            case "$dashes": player.Dashes = FrostHelperImports.GetIntSessionExpressionValue(SessionExpression, level.Session); return;
            default:
                if (Target.StartsWith("$")) {
                    ActionManager.LogError($"Cannot assign to {Target}");
                    return;
                }
                SessionValueKind kind = Target.StartsWith('#') ? SessionValueKind.Counter :
                    Target.StartsWith('@') ? SessionValueKind.Slider :
                    SessionValueKind.Flag;
                string realTarget = Target.TrimStart(['#', '@']).Trim();

                switch (kind) {
                    case SessionValueKind.Flag: level.Session.SetFlag(realTarget, FrostHelperImports.GetBoolSessionExpressionValue(SessionExpression, level.Session)); break;
                    case SessionValueKind.Counter: level.Session.SetCounter(realTarget, FrostHelperImports.GetIntSessionExpressionValue(SessionExpression, level.Session)); break;
                    case SessionValueKind.Slider: level.Session.SetSlider(realTarget, FrostHelperImports.GetFloatSessionExpressionValue(SessionExpression, level.Session)); break;
                }
                return;
        }
    }
}
