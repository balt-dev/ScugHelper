using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using MonoMod.Utils;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/LevelTeleportTrigger")]
public class LevelTeleportTrigger(EntityData data, Vector2 offset, EntityID id) : Trigger(data, offset) {
    internal struct TriggerData {
        internal string? TargetAreaSID;
        internal AreaMode TargetAreaMode;
        internal string LevelName;
        internal Player.IntroTypes IntroType;
        internal string? Flag;
        internal bool FlagState;
        internal Vector2? SpawnLocation;
    }
    internal readonly TriggerData Data = new() {
        TargetAreaSID = data.String("TargetAreaSID"),
        TargetAreaMode = data.Enum("TargetAreaMode", AreaMode.Normal),
        LevelName = data.String("LevelName", ""),
        IntroType = data.Enum("IntroType", Player.IntroTypes.Respawn),
        Flag = data.String("IfFlag"),
        FlagState = data.Bool("FlagState", true),
        SpawnLocation = data.FirstNodeNullable(offset),
    };
    public override void OnEnter(Player player) {
        if (Data.Flag is not null && player.level.Session.GetFlag(Data.Flag) != Data.FlagState) return;
        Scene.OnEndOfFrame += () => {
            if (Data.TargetAreaSID is null) {
                player.level.TeleportTo(player, Data.LevelName, Data.IntroType, Data.SpawnLocation);
            } else {
                AreaData data = AreaData.Get(Data.TargetAreaSID);
                Session session = new() {
                    Area = data.ToKey(Data.TargetAreaMode),
                    Level = Data.LevelName,
                    Dashes = player.level.Session.Dashes,
                    Deaths = player.level.Session.Deaths,
                    Time = player.level.Session.Time,
                    FirstLevel = false,
                    JustStarted = false,
                    OldStats = new(),
                    Inventory = player.level.Session.Inventory,
                    Audio = player.level.Session.Audio
                };
                DynamicData.For(session).Set("BrassBerryCrossLevel", ScugHelperModule.Session.BrassBerryFollowing);
                foreach (var flag in player.level.Session.Flags) session.Flags.Add(flag);
                foreach (var ctr in player.level.Session.Counters) session.Counters.Add(ctr);
                var sourceSliders = (Dictionary<string, Session.Slider>) DynamicData.For(player.level.Session).Get("_Sliders")!;
                var destSliders = (Dictionary<string, Session.Slider>) DynamicData.For(session).Get("_Sliders")!;
                foreach (var kvp in sourceSliders) destSliders.Add(kvp.Key, kvp.Value);
                var levelLoader = Engine.Scene = new LevelLoader(session);
                DynamicData.For(levelLoader).Set("ScugHelper.NaiveTeleport", Data);
            }
        };
    }

    private static readonly MethodInfo m_OrigLoadLevel
        = typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance)!;
    private static ILHook? Levelorig_LoadLevelHook;
    
    [OnLoad] internal static void LoadHooks() {
        Levelorig_LoadLevelHook = new ILHook(m_OrigLoadLevel, ILLevelorig_LoadLevel);
        On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStartLevel;
    }
    [OnUnload] internal static void UnloadHooks() {
        Levelorig_LoadLevelHook?.Dispose();
        On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStartLevel;
    }
    
    private static readonly Player.IntroTypes IntroTypeTeleport = (Player.IntroTypes) (-0x5C60000);
    private static TriggerData? HackyDataSmuggle;

    private static void OnLevelLoaderStartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
        var dynData = DynamicData.For(self);
        if (dynData.Get("ScugHelper.NaiveTeleport") is TriggerData data) {
            self.PlayerIntroTypeOverride = IntroTypeTeleport;
            HackyDataSmuggle = data;
        }
        orig(self);
    }
        
    private static void ILLevelorig_LoadLevel(ILContext il) {
        ILCursor cur = new(il);
        cur.GotoNext(MoveType.After,
            static instr => instr.MatchStsfld<SpotlightWipe>("FocusPoint")
        );
        ILLabel label = cur.DefineLabel();
        static bool Delegate(Level level, Player.IntroTypes type) {
            if (type != IntroTypeTeleport) return false;
            Player player = level.Tracker.GetEntity<Player>() ?? throw new NullReferenceException("How the hell is the player null *here*???");
            if (HackyDataSmuggle is not TriggerData tp) throw new NullReferenceException("How the hell is the trigger data null???");
            player.StateMachine.State = Player.StDummy;
            level.OnEndOfFrame += () => level.TeleportTo(player, tp.LevelName, tp.IntroType, tp.SpawnLocation);
            
            return false;
        }
        cur.EmitLdarg0();
        cur.EmitLdarg1();
        cur.EmitDelegate(Delegate);
        cur.EmitBrtrue(label);
        cur.GotoNext(MoveType.After,
            static instr => instr.MatchCallOrCallvirt<Level>("DoScreenWipe")
        );
        
        cur.MarkLabel(label);
    }
}
