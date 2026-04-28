using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

/// <summary>
/// Abstract interface for Actions.
/// Implementors must not store state on the class itself, and must only store it on the level's session data or within existing entities.
/// Any non-static fields must only be used for caching.
///
/// Action entites are never actually placed into the level.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Method to be called when this Action is alerted.
    /// </summary>
    public virtual void Alert(Level level) { }
    /// <summary>
    /// Method to be called when this Action is updated every tick.
    /// </summary>
    public virtual void ActionUpdate(Level level) { }
    /// <summary>
    /// Helper method to get the groups of an action.
    /// </summary>
    public static sealed string[] GetGroups(EntityData data) => GetGroups(data.String("Groups", ""));
    public static sealed string[] GetGroups(string str) => (str ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    /// <summary>
    /// Helper method to get the targets of something that will trigger action groups.
    /// </summary>
    public static sealed string[] GetTargets(EntityData data) {
        var res = data.String("Targets", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (res.Any((str) => str.StartsWith("#")))
            throw new Exception($"Target of action with ID {data.ID} must not start with #, as those are reserved for builtin events.");
        return res;
    }
}
