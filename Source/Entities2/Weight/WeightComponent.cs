using System;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Weight;

[Tracked]
internal class WeightComponent(Func<Weight> weight) : Component(false, false) {
    internal readonly Func<Weight> GetWeight = weight;
}