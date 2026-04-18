using System;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;
#nullable enable

/// <summary>
/// Component for entities to listen for an alerted Action.
/// </summary>
[Tracked(true)]
public class ActionListener(string[] groups, Action<Level> callback) : Component(false, false)
{
    /// <summary> The groups this component is listening for alerts in. </summary>
    public string[] Groups { get; protected init; } = groups;
    /// <summary> The callback for when a listened group is alterted. </summary>
    public Action<Level> Alert { get; protected init; } = callback;
}
