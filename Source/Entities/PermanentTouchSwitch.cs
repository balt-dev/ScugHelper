using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections.Generic;
using MonoMod.Cil;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SaveTouchSwitchesTrigger")]
public class SaveTouchSwitchesTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    static List<EntityID> ToSave = [];
    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (scene is not Level level) return;
        foreach (TouchSwitch sw in scene.Tracker.GetEntities<TouchSwitch>()) {
            if (level.Session.GetFlag($"ScugHelper.TouchSwitch.{sw.SourceId}"))
                sw.Switch.Activate();
        }
    }
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        foreach (var id in ToSave)
        {
            player.level.Session.SetFlag($"ScugHelper.TouchSwitch.{id}");
        }
    }
    [OnLoad]
    internal static void LoadHooks()
    {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        On.Celeste.TouchSwitch.TurnOn += OnTurnOn;
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) => ToSave = [];

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        On.Celeste.TouchSwitch.TurnOn -= OnTurnOn;
    }

    private static void OnTurnOn(On.Celeste.TouchSwitch.orig_TurnOn orig, TouchSwitch self)
    {
        orig(self);
        ToSave.Add(self.SourceId);
    }
}
