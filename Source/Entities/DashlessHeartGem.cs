using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections.Generic;
using MonoMod.Cil;
namespace Celeste.Mod.ScugHelper.Entities;

[TrackedAs(typeof(HeartGem))]
[CustomEntity("ScugHelper/DashlessHeartGem")]
public class DashlessHeartGem(EntityData data, Vector2 offset) : HeartGem(data, offset)
{
    public static void LoadHooks()
    {
        On.Celeste.HeartGem.OnPlayer += OnPlayerHook;
    }
    public static void UnloadHooks()
    {
        On.Celeste.HeartGem.OnPlayer -= OnPlayerHook;
    }

    private static void OnPlayerHook(On.Celeste.HeartGem.orig_OnPlayer orig, HeartGem self, Player player)
    {
        if (self is DashlessHeartGem && !(self.collected || (self.Scene as Level).Frozen))
        {
            self.Collect(player);
            return;
        }
        orig(self, player);
    }
}
