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
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

using Callback = Action<Level>;

readonly struct ActionMapEntry(Callback? action = null, float? delay = null, EntityData? data = null, Action? update = null, bool immediate = false)
{
    public readonly Callback? Action = action;
    public readonly float? Delay = delay;
    public readonly EntityData? AssociatedData = data;
    public readonly Action? Update = update;
    public readonly bool Immediate = immediate;
}

public static class ActionManager
{
    private static readonly Dictionary<string, List<ActionMapEntry>> actionMap = [];
    private static readonly HashSet<int> idSet = [];
    internal static readonly List<Callback> updaters = [];
    internal static readonly List<Entity> globalEnts = [];

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
            try {
                if (level is Level lv)
                    foreach (ActionListener listener in lv.Tracker.GetComponents<ActionListener>())
                        if (listener.Groups.Contains(group))
                            listener.Alert(level);
            } catch (KeyNotFoundException) { }
        }
    }

    private static IEnumerator ActionBuffer(ActionMapEntry entry, Level? level = null) {
        if (level is null) {
            if (Engine.Scene is not Level lv) yield break;
            level = lv;
        }
        if (!entry.Immediate)
            yield return entry.Delay != null && entry.Delay > 0.01f ? entry.Delay : null;
        entry.Action?.Invoke(level);
    }

    private enum ConstructorKind { Bare, TwoArg, ThreeArg }

    private static readonly ConcurrentDictionary<Type, (ConstructorInfo, ConstructorKind)> ConstructorCache = [];
    private static ActionDummy dummy = [];

    internal static void RegisterAction(Level level, EntityData data, LevelData room) {
        var ty = ScugHelperModule.GetTypeOfEntity(data);
        if (!ty?.GetInterfaces().Contains(typeof(IAction)) ?? true) return;
        int id = data.ID;
        level.Session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = id });
        string[] groups = IAction.GetGroups(data);
        float? delay = data.Float("Delay");
        if (delay <= 0) delay = null;
        object? action = TryConstructEntity(data, room, out _);
        if (action is not Entity actionEnt) throw new Exception($"Constructor for action type {ty} must return an implementer of Entity.");
        actionEnt.Added(level);
        actionEnt.Awake(level);
        if (action is not IAction iAction) throw new Exception($"Constructor for action type {ty} must return an implementer of IActor.");
        if (!idSet.Add(id)) return;
        updaters.Add(iAction.ActionUpdate);
        foreach (string group in groups) {
            if (!actionMap.TryGetValue(group, out var actions))
                actionMap.Add(group, actions = []);
            actions.Add(new(iAction.Alert, delay, data, immediate: data?.Bool("Immediate") ?? false));
        }
        if (action is GlobalTriggerFlagListener listener) {
            foreach (var entData in room.Triggers) {
                if (TryConstructEntity(entData, room, out _) is not Trigger trigger) {
                    Logger.Warn(nameof(ScugHelperModule), $"Failed to construct: {entData}");
                    continue;
                }
                if (trigger.Collider.Collide(data.Position + room.Position)) {
                    level.Session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = entData.ID });
                    listener.triggers.Add(trigger);
                    globalEnts.Add(trigger);
                    listener.Added(level);
                    listener.Awake(level);
                }
            }
        }
        if (action is GlobalTriggerActionListener actListener) {
            foreach (var entData in room.Triggers) {
                if (TryConstructEntity(entData, room, out _) is not Trigger trigger) {
                    Logger.Warn(nameof(ScugHelperModule), $"Failed to construct: {entData}");
                    continue;
                }
                if (trigger.Collider.Collide(data.Position + room.Position)) {
                    level.Session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = entData.ID });
                    actListener.triggers.Add(trigger);
                    globalEnts.Add(trigger);
                    trigger.Added(level);
                    trigger.Awake(level);
                }
            }
        }
        if (action is GlobalEntityActionListener entActListener) {
            foreach (var entData in room.Entities) {
                if (TryConstructEntity(entData, room, out _) is not Entity entity) {
                    Logger.Warn(nameof(ScugHelperModule), $"Failed to construct: {entData}");
                    continue;
                }
                if (entity.Get<PlayerCollider>() is not PlayerCollider collider) continue;
                if (entActListener.Collider.Collide(entData.Position + room.Position)) {
                    level.Session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = entData.ID });
                    entActListener.colliders.Add(collider);
                    globalEnts.Add(entity);
                    entity.Added(level);
                    entity.Awake(level);
                }
            }
        }
        if (action is EntityGlobalizer entGlobalizer) {
            foreach (var entData in room.Entities) {
                if (TryConstructEntity(entData, room, out _) is not Entity entity) continue;
                if (entGlobalizer.Collider.Collide(entData.Position + room.Position)) {
                    level.Session.DoNotLoad.Add(new EntityID() { Level = room.Name, ID = entData.ID });
                    globalEnts.Add(entity);
                    entity.Added(level);
                    entity.Awake(level);
                }
            }
        }
    }

    public static object? TryConstructEntity(EntityData data, LevelData room, out Type? type) {
        type = null;
        if (ScugHelperModule.GetTypeOfEntity(data) is not Type ty) return null;
        if (!ty.IsSubclassOf(typeof(Entity))) return null;
        type = ty;
        ConstructorKind kind = ConstructorKind.Bare;
        ConstructorInfo? constructor;
        if (ConstructorCache.TryGetValue(ty, out var pair))
            (constructor, kind) = pair;
        else {
            ConstructorInfo? val = ty.GetConstructor([]);
            if (val is null) {
                kind = ConstructorKind.TwoArg;
                val = ty.GetConstructor([typeof(EntityData), typeof(Vector2)]);
            }
            if (val is null) {
                kind = ConstructorKind.ThreeArg;
                val = ty.GetConstructor([typeof(EntityData), typeof(Vector2), typeof(EntityID)]);
            }
            if (val is not ConstructorInfo constr) {
                Logger.Warn(nameof(ScugHelperModule), $"Could not find a constructor for entity with type {ty}.");
                return null;
            }
            ConstructorCache.TryAdd(ty, (constr, kind));
            constructor = constr;
        }
        return (Entity)(kind switch {
            ConstructorKind.Bare => constructor.Invoke([]),
            ConstructorKind.TwoArg => constructor.Invoke([data, room.Position]),
            ConstructorKind.ThreeArg => constructor.Invoke([data, room.Position, new EntityID(room.Name, data.ID)]),
        });
    }

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += OnLevelUpdate;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= OnLevelUpdate;
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        orig(self);
        dummy.Scene = self;
        dummy.Update();
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        if (isFromLoader) {
            actionMap.Clear();
            idSet.Clear();
            updaters.Clear();
            globalEnts.Clear();
            MapData data = level.Session.MapData;
            foreach (var room in data.Levels)
                foreach (var entData in room.Entities)
                    RegisterAction(level, entData, room);
        }

        foreach (var trigger in globalEnts)
            trigger.Added(level);
        if (isFromLoader)
            AlertActions(["#InitActions"], level);
        AlertActions(["#LoadLevel"], level);
    }

    [Command("alert", "Alerts a specified action group.")]
    internal static void CmdTriggerActionGroup(string group) {
        AlertActions([group], null);
    }

    [Command("actions", "Shows all action groups. An optional first argument searches for groups with a given string in their name.")]
    internal static void CmdShowActionGroups(string? search = null) {
        Engine.Commands.Log($"Action groups:");
        foreach ((string key, List<ActionMapEntry> value) in actionMap.AsEnumerable()) {
            if (search is null || key.Contains(search)) {
                Engine.Commands.Log($"  {key}:");
                foreach (var entry in value) {
                    Engine.Commands.Log($"    {entry.AssociatedData?.ID}: {entry.AssociatedData?.Name}");
                    Engine.Commands.Log($"    {{{string.Join(", ", entry.AssociatedData?.Values.AsEnumerable().Select((pair) => $"{pair.Key}: {pair.Value}") ?? [])}}}");
                }
            }
        }
    }

    internal static void LogError(string message) {

        Logger.Error(nameof(ScugHelperModule), $"Action error: {message}");
        Engine.Commands.Open = true;
        Engine.Commands.Log($"Action error: {message}", Color.Red);
    }

    [Command("globalents", "Shows all global entities")]
    internal static void CmdShowGlobalEnts() {
        Engine.Commands.Log($"Global Entities:");
        foreach (Entity ent in globalEnts) {
            Engine.Commands.Log($"  {ent.SourceId}: {ent}");
        }
    }
}

[CustomEntity("ScugHelper/ActionDummy")]
internal class ActionDummy() : Entity()
{
    public override void Update() {
        base.Update();
        Level lv = SceneAs<Level>();
        ActionManager.AlertActions(["#Tick"], lv);
        foreach (var updater in ActionManager.updaters) updater.Invoke(lv);
    }
}
