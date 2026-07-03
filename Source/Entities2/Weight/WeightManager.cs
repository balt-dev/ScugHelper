
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Weight;

public static class WeightManager {
    public static IReadOnlyDictionary<Type, Weight> DefaultWeights => defaultWeights;
    private static readonly Dictionary<Type, Weight> defaultWeights = [
        new(typeof(Player), (Weight) 1),
        new(typeof(TheoCrystal), (Weight) 1.5),
        new(typeof(Glider), (Weight) 0.2),
        new(typeof(RefillCrystal), (Weight) 1.3),
        new(typeof(Seeker), (Weight) 1.8),
        new(typeof(Puffer), (Weight) 1.2),
        new(typeof(TungstenCube), (Weight) 250)
    ];
    
    internal static bool _InteropAddDefaultWeight(Type entityType, float weight) {
        if (defaultWeights.ContainsKey(entityType)) return false;
        defaultWeights[entityType] = (Weight)weight;
        return true;
    }

    public static Weight? WeightOf(Entity entity) {
        { if (entity.Get<WeightComponent>() is { } comp) return comp.GetWeight(); }
        { if (
            entity.Scene.Tracker.GetEntity<WeightController>() is { } man && (
                from name in Utils.GetNamesOfEntity(entity)
                let val = man.CustomWeights.GetValueOrNull(name)
                where val is not null
                select val
            ).First() is Weight weight
        ) return weight; }
        { if (DefaultWeights.TryGetValue(entity.GetType(), out var weight))
            return weight; }
        return null;
    }
    
    [Command("weights", "Gets all weights set in the current level.")]
    internal static void CmdWeights() {
        var weights = Engine.Scene.Tracker.GetEntity<WeightController>() is { } man ? man.CustomWeights.Clone() : [];
        { foreach (var kvp in DefaultWeights) {
            foreach (string name in Utils.GetNamesOfEntity(kvp.Key))
                weights.TryAdd(name, kvp.Value);
        } }
        { foreach (var kvp in weights) {
            Engine.Commands.Log($"{kvp.Key}: {(double)kvp.Value}", Color.LightBlue);
        } }
    }
}
