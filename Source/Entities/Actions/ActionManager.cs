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
    internal static readonly List<Callback> updaters = [];
    internal static readonly List<Trigger> globalTriggers = [];

    /// <summary>
    /// Triggers the callback of any actions with any of the given groups.
    /// Actions will always wait at least one frame to be triggered.
    /// </summary>
    public static void AlertActions(string[] groups, Level? level)
    {
        foreach (string group in groups)
        {
            if (actionMap.TryGetValue(group, out var actions))
                foreach (var action in actions)
                    dummy.Add(new Coroutine(ActionBuffer(action, level)));
            if (level is Level lv)
                foreach (ActionListener listener in lv.Tracker.GetComponents<ActionListener>())
                    if (listener.Groups.Contains(group))
                        listener.Alert(level);
        }
    }

    private static IEnumerator ActionBuffer(ActionMapEntry entry, Level? level = null)
    {
        if (level is null)
        {
            if (Engine.Scene is not Level lv) yield break;
            level = lv;
        }
        yield return entry.Delay != null && entry.Delay > 0.01f ? entry.Delay : null;
        entry.Action?.Invoke(level);
    }

    private enum ConstructorKind { Bare, TwoArg, ThreeArg }

    private static readonly ConcurrentDictionary<Type, (ConstructorInfo, ConstructorKind)> ConstructorCache = [];
    private static ActionDummy dummy = [];

    internal static void RegisterAction(Session session, EntityData data, LevelData room)
    {
        var ty = GetTypeOfEntity(data);
        if (!ty?.GetInterfaces().Contains(typeof(IAction)) ?? true) return;
        int id = data.ID;
        session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = id });
        string[] groups = IAction.GetGroups(data);
        float? delay = data.Float("Delay");
        if (delay <= 0) delay = null;
        object? action = TryConstructEntity(data, room, out _);
        if (action is not IAction iAction) throw new Exception($"Constructor for action type {ty} must return an implementer of IActor.");
        if (!idSet.Add(id)) return;
        updaters.Add(iAction.ActionUpdate);
        foreach (string group in groups) {
            if (!actionMap.TryGetValue(group, out var actions))
                actionMap.Add(group, actions = []);
            actions.Add(new(iAction.Alert, delay, data));
        }
        if (action is GlobalTriggerFlagListener listener) {
            foreach (var entData in room.Triggers) {
                if (TryConstructEntity(entData, room, out _) is not Trigger trigger) continue;
                if (trigger.Collider.Collide(data.Position + room.Position)) {
                    session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = entData.ID });
                    listener.triggers.Add(trigger);
                    globalTriggers.Add(trigger);
                }
            }
        }
    }

    private static Type? GetTypeOfEntity(EntityData data)
    {
        var type = EntityRegistry.GetKnownTypesFromSid(data.Name).AsEnumerable().FirstOrDefault((Type?)null);
        if (type is not Type ty)
        {
            Logger.Warn(nameof(ScugHelperModule), $"SID {data.Name} of entity with ID {data.ID} does not correspond to any known types.");
        }
        return type;
    }

    private static object? TryConstructEntity(EntityData data, LevelData room, out Type? type)
    {
        type = null;
        if (GetTypeOfEntity(data) is not Type ty) return null;
        if (!ty.IsSubclassOf(typeof(Entity))) return null;
        type = ty;
        ConstructorKind kind = ConstructorKind.Bare;
        ConstructorInfo? constructor;
        if (ConstructorCache.TryGetValue(ty, out var pair))
            (constructor, kind) = pair;
        else
        {
            ConstructorInfo? val = ty.GetConstructor([]);
            if (val is null)
            {
                kind = ConstructorKind.TwoArg;
                val = ty.GetConstructor([typeof(EntityData), typeof(Vector2)]);
            }
            if (val is null)
            {
                kind = ConstructorKind.ThreeArg;
                val = ty.GetConstructor([typeof(EntityData), typeof(Vector2), typeof(EntityID)]);
            }
            if (val is not ConstructorInfo constr) throw new Exception($"Action type {ty} must have a valid constructor.");
            ConstructorCache.TryAdd(ty, (constr, kind));
            constructor = constr;
        }
        return (Entity) (kind switch {
            ConstructorKind.Bare => constructor.Invoke([]),
            ConstructorKind.TwoArg => constructor.Invoke([data, room.Position]),
            ConstructorKind.ThreeArg => constructor.Invoke([data, room.Position, new EntityID(room.Name, data.ID)]),
        });
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
        if (isFromLoader) {
            actionMap.Clear();
            idSet.Clear();
            updaters.Clear();
            globalTriggers.Clear();
            MapData data = level.Session.MapData;
            foreach (var room in data.Levels)
                foreach (var entData in room.Entities)
                    RegisterAction(level.Session, entData, room);
        }

        foreach (var trigger in globalTriggers)
            trigger.Added(level);
        if (isFromLoader)
            AlertActions(["#InitActions"], level);
        AlertActions(["#LoadLevel"], level);
    }

    [Command("alert", "Alerts a specified action group.")]
    internal static void CmdTriggerActionGroup(string group)
    {
        AlertActions([group], null);
    }

    [Command("actions", "Shows all action groups. An optional first argument searches for groups with a given string in their name.")]
    internal static void CmdShowActionGroups(string? search = null) {
        Engine.Commands.Log($"Action groups:");
        foreach ((string key, List<ActionMapEntry> value) in actionMap.AsEnumerable()) {
            if (search is null || key.Contains(search)) {
                Engine.Commands.Log($"  {key}:");
                foreach (var entry in value)
                {
                    Engine.Commands.Log($"    {entry.AssociatedData?.ID}: {entry.AssociatedData?.Name}");
                    Engine.Commands.Log($"    {{{string.Join(", ", entry.AssociatedData?.Values.AsEnumerable().Select((pair) => $"{pair.Key}: {pair.Value}") ?? [])}}}");
                }
            }
        }
    }

    [Command("sessionvars", "Shows currently set flags, counters, and sliders. An optional first argument searches for values with a given string in their name.")]
    internal static void ShowValues(string? search = null) {
        if (Engine.Scene is not Level lv) return;
        Session session = lv.Session;
        Engine.Commands.Log($"Flags:");
        foreach (string flag in session.Flags)
            if (search is null || flag.Contains(search))
                Engine.Commands.Log($"- {flag}");
        Engine.Commands.Log($"Counters:");
        foreach (Session.Counter counter in session.Counters)
            if (search is null || counter.Key.Contains(search))
                Engine.Commands.Log($"- {counter.Key}: {counter.Value}");
        Engine.Commands.Log($"Sliders:");
        foreach (Session.Slider slider in session.Sliders.Values)
            if (search is null || slider.Name.Contains(search))
                Engine.Commands.Log($"- {slider.Name}: {slider.Value}");
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
