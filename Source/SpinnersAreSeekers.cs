using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper;

public enum SpinnerSeekerState
{
    Off = 0,
    Normal = 1,
    Fragile = 2
}

internal static class SpinnersAreSeekers
{
    [OnLoad]
    internal static void LoadHooks()
    {
        On.Monocle.Entity.Added += OnEntityAdded;
    }
    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Monocle.Entity.Added -= OnEntityAdded;
    }

    private static void OnEntityAdded(On.Monocle.Entity.orig_Added orig, Monocle.Entity self, Monocle.Scene scene)
    {
        orig(self, scene);

        if (ScugHelperModule.Settings.SpinnersAreSeekers != SpinnerSeekerState.Off && (
            self is CrystalStaticSpinner or DustStaticSpinner
        ))
        {
            scene.Add(ScugHelperModule.Settings.SpinnersAreSeekers switch {
                SpinnerSeekerState.Off => throw new InvalidOperationException("Spinner seeker setting is off and on at the same time somehow."),
                SpinnerSeekerState.Normal => new Seeker(self.Position, []),
                SpinnerSeekerState.Fragile => new FragileSeeker(self.Position, []),
                _ => throw new InvalidOperationException("Spinner seeker setting is set to an invalid state."),
            });
            self.RemoveSelf();
        }
    }
}