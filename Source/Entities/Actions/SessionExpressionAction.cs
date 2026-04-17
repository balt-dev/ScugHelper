using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

[CustomEntity("ScugHelper/SessionExpressionAction")]
public class SessionExpressionAction : Entity, IAction
{

internal enum ValueKind { Flag, Counter, Slider };
    private readonly object SessionExpresion;
    private readonly string Target;
    private readonly SessionValueKind ValueKind;
    public SessionExpressionAction(EntityData data, Vector2 _): base() {
        if (!FrostHelperImports.IsLoaded)
            throw new Exception("FrostHelper must be loaded to use session expression actions.");
        ValueKind = data.Enum<ValueKind>("ValueKind");
    }
    public void Alert(Level level)
    {
    }
}

