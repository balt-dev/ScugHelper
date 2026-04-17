using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.Registry;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

using Callback = Action<Level>;

readonly struct ActionMapEntry(Callback? action = null, float? delay = null, EntityData? data = null, Action? update = null) {
    public readonly Callback? Action = action;
    public readonly float? Delay = delay;
    public readonly EntityData? AssociatedData = data;
    public readonly Action? Update = update;
}

public static class ActionManager
{
    private static readonly Dictionary<string, List<ActionMapEntry>> actionMap = [];
    private static readonly HashSet<int> idSet = [];
    internal static readonly List<Action<Level>> updaters = [];

    /// <summary>
    /// Triggers the callback of any actions with any of the given groups.
    /// Actions will always wait at least one frame to be triggered.
    /// </summary>
    public static void AlertActions(string[] groups, Level? level)
    {
        foreach (string group in groups)
            if (actionMap.TryGetValue(group, out var actions))
                foreach (var action in actions)
                    dummy.Add(new Coroutine(ActionBuffer(action, level)));
    }

    private static IEnumerator ActionBuffer(ActionMapEntry entry, Level? level = null)
    {
        if (level is null)
        {
            if (Engine.Scene is not Level lv) yield break;
            level = lv;
        }
        if (entry.AssociatedData is not EntityData data) yield break;
        yield return entry.Delay != null && entry.Delay > 0.01f ? entry.Delay : null;
        entry.Action?.Invoke(level);
    }

    private static readonly ConcurrentDictionary<Type, (ConstructorInfo, bool)> ConstructorCache = [];
    private static ActionDummy dummy = [];

    internal static void RegisterAction(Session session, EntityData data)
    {
        var type = EntityRegistry.GetKnownTypesFromSid(data.Name).AsEnumerable().FirstOrDefault((Type?)null);
        if (type is not Type ty)
        {
            Logger.Warn(nameof(ScugHelperModule), $"SID {data.Name} of entity with ID {data.ID} does not correspond to any known types.");
            return;
        }
        if (!ty.GetInterfaces().Contains(typeof(IAction))) return;
        int id = data.ID;
        session.DoNotLoad.Add(new EntityID() { Level = session.Level, ID = id });
        string[] groups = IAction.GetGroups(data);
        float? delay = data.Float("Delay");
        if (delay <= 0) delay = null;
        ConstructorInfo? constructor = null;
        bool takesData = false;
        if (ConstructorCache.TryGetValue(ty, out var pair))
            (constructor, takesData) = pair;
        else
        {
            ConstructorInfo? val = ty.GetConstructor([]);
            if (val is null)
            {
                takesData = true;
                val = ty.GetConstructor([typeof(EntityData), typeof(Vector2)]);
            }
            if (val is not ConstructorInfo constr) throw new Exception($"Action type {ty} must have a constructor of either () or (EntityData, Vector2).");
            ConstructorCache.TryAdd(ty, (constr, takesData));
            constructor = constr;
        }
        object action = takesData ? constructor.Invoke([data, Vector2.Zero]) : constructor.Invoke([]);
        if (action is not IAction iAction) throw new Exception($"Constructor for action type {ty} must return an implementer of IActor.");
        if (!idSet.Add(id)) return;
        updaters.Add(iAction.ActionUpdate);
        foreach (string group in groups) {
            if (!actionMap.TryGetValue(group, out var actions))
                actionMap.Add(group, actions = []);
            actions.Add(new(iAction.Alert, delay, data));
        }
    }

    internal static void LoadHooks() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        ActionHooks.LoadHooks();
    }
    internal static void UnloadHooks() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        ActionHooks.UnloadHooks();
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        dummy.RemoveSelf();
        level.Add(dummy = []);
        if (!isFromLoader) return;
        actionMap.Clear();
        idSet.Clear();
        MapData data = level.Session.MapData;
        foreach (var room in data.Levels)
            foreach (var entData in room.Entities)
                RegisterAction(level.Session, entData);
        AlertActions(["#LoadLevel"], level);
    }

    [Command("alert", "Alerts a specified action group.")]
    internal static void CmdTriggerActionGroup(string group)
    {
        AlertActions([group], null);
    }

    [Command("actions", "Shows all action groups.")]
    internal static void CmdShowActionGroups() {
        Engine.Commands.Log($"Action groups:");
        foreach ((string key, List<ActionMapEntry> value) in actionMap.AsEnumerable()) {
            Engine.Commands.Log($"  {key}:");
            foreach (var entry in value)
            {
                Engine.Commands.Log($"    {entry.AssociatedData?.ID}: {entry.AssociatedData?.Name}");
                Engine.Commands.Log($"    {{{string.Join(", ", entry.AssociatedData?.Values.AsEnumerable().Select((pair) => $"{pair.Key}: {pair.Value}") ?? [])}}}");
            }
        }
    }

    internal static void LogError(string message)
    {

        Logger.Error(nameof(ScugHelperModule), $"Action error: {message}");
        Engine.Commands.Open = true;
        Engine.Commands.Log($"Action error: {message}", Color.Red);
    }
}

[CustomEntity("ScugHelper/ActionDummy")]
internal class ActionDummy(): Entity() {
    public override void Update() {
        base.Update();
        Level lv = SceneAs<Level>();
        ActionManager.AlertActions(["#Tick"], lv);
        foreach (var updater in ActionManager.updaters) updater.Invoke(lv);
    }
}
