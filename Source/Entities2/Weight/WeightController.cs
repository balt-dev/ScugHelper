using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Weight;

[Tracked]
[CustomEntity("ScugHelper/WeightController")]
public class WeightController(EntityData data, Vector2 offset) : Entity() {
    public Dictionary<string, Weight> CustomWeights = [];
    internal string SerializedWeights = data.String("Weights", "");

    public override void Added(Scene scene) {
        if (scene is not Level level) return;
        CustomWeights = [];
        foreach (string entry in data.String("Weights").Split(",")) {
            var split = entry.Split(":");
            if (split.Count() != 2) { ThrowPostcard(level.Session, entry); return; }
            if (!double.TryParse(split[1], out double weight)) { ThrowPostcard(level.Session, entry); return; }
            CustomWeights.Add(split[0], (Weight)weight);
        }
    }

    private static void ThrowPostcard(Session session, string weight) {
        LevelEnter.ErrorMessage = Dialog.Get("ScugHelper_postcard_invalidWeight").Replace("((entry))", weight);
        Engine.Scene = new LevelEnter(session, false);
    }
}
