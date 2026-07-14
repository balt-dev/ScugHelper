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
public class LevelTeleportTrigger : Trigger {
    internal struct TriggerData {
        internal AreaData? TargetAreaData;
        internal AreaMode TargetAreaMode;
        internal string LevelName;
        internal Player.IntroTypes IntroType;
        internal string? Flag;
        internal bool FlagState;
        internal Vector2? SpawnLocation;
    }
    internal readonly TriggerData Data;
    public LevelTeleportTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
        AreaData? areaData = null;
        if (data.String("TargetAreaSID") is {} sid)
            areaData = AreaData.Get(sid)
                ?? throw new InvalidOperationException($"Invalid target SID: {data.String("TargetAreaSID")}");
        Data = new() {
            TargetAreaData = areaData,
            TargetAreaMode = data.Enum("TargetAreaMode", AreaMode.Normal),
            LevelName = data.String("LevelName", ""),
            IntroType = data.Enum("IntroType", Player.IntroTypes.Respawn),
            Flag = data.String("IfFlag"),
            FlagState = data.Bool("FlagState", true),
            SpawnLocation = data.FirstNodeNullable(offset - data.Level.Position),
        };
    }
    public override void OnEnter(Player player) {
        if (Data.Flag is not null && player.level.Session.GetFlag(Data.Flag) != Data.FlagState) return;
        Scene.OnEndOfFrame += () => {
            Level level = player.level ?? Scene as Level ?? Engine.Scene as Level ?? throw new InvalidOperationException("Tried to level teleport while not currently in a level.");
            if (Data.TargetAreaData is {} data) {
                var Area = data.ToKey(Data.TargetAreaMode);
                var Level = Data.LevelName;
                var Dashes = level.Session?.Dashes ?? 0;
                var Deaths = level.Session?.Deaths ?? 0;
                var Time = level.Session?.Time ?? 0;
                var FirstLevel = false;
                var JustStarted = false;
                AreaStats OldStats = new();
                var Inventory = level.Session?.Inventory ?? PlayerInventory.Default;
                var Audio = level.Session?.Audio;
                Session session = new() {
                    Area = Area,
                    Level = Level,
                    Dashes = Dashes,
                    Deaths = Deaths,
                    Time = Time,
                    FirstLevel = FirstLevel,
                    JustStarted = JustStarted,
                    OldStats = OldStats,
                    Inventory = Inventory,
                    Audio = Audio
                };
                DynamicData.For(session).Set("BrassBerryCrossLevel", ScugHelperModule.Session.BrassBerryFollowing);
                if (level.Session is not null) {
                    foreach (var flag in level.Session.Flags) session.Flags.Add(flag);
                    foreach (var ctr in level.Session.Counters) session.Counters.Add(ctr);
                    var sourceSliders = (Dictionary<string, Session.Slider>)DynamicData.For(level.Session).Get("_Sliders")!;
                    var destSliders = (Dictionary<string, Session.Slider>)DynamicData.For(session).Get("_Sliders")!;
                    foreach (var kvp in sourceSliders) destSliders.Add(kvp.Key, kvp.Value);
                }
                var levelLoader = Engine.Scene = new LevelLoader(session);
                DynamicData.For(levelLoader).Set("ScugHelper.NaiveTeleport", Data);
            } else {
                level.TeleportTo(player, Data.LevelName, Data.IntroType, Data.SpawnLocation);
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
