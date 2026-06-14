using MonoMod.Cil;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Monocle;
using Celeste.Mod.Helpers;
using Mono.Cecil.Cil;
using System;
using Mono.Cecil;
using Celeste.Mod.UI;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;

namespace Celeste.Mod.ScugHelper;

public static class MapHider {
    private static readonly string HiddenLevelSet = "ScugHelper/ScugHelperTest";

    private static ILHook? hookOnLevelSetSwitch;
    private static ILHook? hookMapSearchReloadItems;
    private static ILHook? hookMapListReloadItems;
    private static ILHook? hookMapListCreateMenu;
    private static ILHook? hookLevelSetPicker;

    [OnLoad]
    internal static void LoadHooks() {
        if (!HookUtils.TryDisableInlining(typeof(OuiHelper_ChapterSelect_LevelSet).GetMethod("Enter")!.GetStateMachineTarget()))
            throw new Exception("Failed to disable inlining for hiding maps");
        hookOnLevelSetSwitch = new ILHook(typeof(OuiHelper_ChapterSelect_LevelSet).GetMethod("Enter")!.GetStateMachineTarget()!, modLevelSetSwitch);
        hookLevelSetPicker = new ILHook(
            typeof(Everest).Assembly.GetType("Celeste.Mod.UI.OuiFileSelectSlotLevelSetPicker")!.GetMethod("changeStartingLevelSet", BindingFlags.NonPublic | BindingFlags.Instance)!,
            modFileSelectChangeStartingLevelSet
        );
        hookMapSearchReloadItems = new ILHook(
            typeof(OuiMapSearch).GetMethod("ReloadItems", BindingFlags.NonPublic | BindingFlags.Instance)!,
            modMapSearch);
        hookMapListReloadItems = new ILHook(
            typeof(OuiMapList).GetMethod("ReloadItems", BindingFlags.NonPublic | BindingFlags.Instance)!, modMapListReloadItems
        );
        hookMapListCreateMenu = new ILHook(
            typeof(OuiMapList).GetMethod("CreateMenu", BindingFlags.NonPublic | BindingFlags.Instance)!, modMapListCreateMenu
        );
        On.Celeste.SaveData.AfterInitialize += onSaveDataAfterInitialize;
        Everest.Events.Level.OnExit += OnLevelOnExit;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        hookOnLevelSetSwitch?.Dispose();
        hookLevelSetPicker?.Dispose();
        hookMapSearchReloadItems?.Dispose();
        hookMapListReloadItems?.Dispose();
        hookMapListCreateMenu?.Dispose();
        On.Celeste.SaveData.AfterInitialize -= onSaveDataAfterInitialize;
        Everest.Events.Level.OnExit -= OnLevelOnExit;
    }
    
    private static void modLevelSetSwitch(ILContext il) {
        ILCursor cursor = new(il);

        // target check: areaData.LevelSet != levelSet
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloc(6), // AreaData getting considered
            instr => instr.MatchCall(typeof(AreaData).Assembly.GetType("Celeste.AreaDataExt")!, "GetLevelSet") || instr.MatchCallvirt<AreaData>("get_LevelSet"),
            instr => instr.MatchLdloc(3), // current level set
            instr => instr.MatchCall<string>("op_Inequality"))
        ) throw new Exception("Failed to match areaData.LevelSet != levelSet for hiding maps");

        // becomes: areaData.LevelSet != levelSet && !IsCollabLevelSet(areaData.LevelSet)
        cursor.Emit(OpCodes.Ldloc_S, (byte) 6);
        cursor.EmitDelegate<Func<bool, AreaData, bool>>(HideScugHelper);
    }

    private static bool HideScugHelper(bool orig, AreaData areaData) {
        return orig && areaData.LevelSet != HiddenLevelSet;
    }
    
    private static bool HideScugHelper(bool orig, string levelSet) {
        return orig && levelSet != HiddenLevelSet;
    }

    private static void modFileSelectChangeStartingLevelSet(ILContext il) {
        ILCursor cursor = new(il);

        // set ourselves just after the moving operation in changeStartingLevelSet.
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdsfld<AreaData>("Areas"),
            instr => instr.MatchLdloc(0),
            instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference)!.Name == "get_Item")
        ) throw new Exception("Failed to match levelset move for hiding maps");

        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(SkipScugHelperPicker);
        cursor.Emit(OpCodes.Stloc_0);
    }

    private static int SkipScugHelperPicker(int id, int direction) {
        string currentLevelSet = AreaData.Areas[id].LevelSet;

        // repeat the move until the current level set isn't a collab level set anymore.
        if (currentLevelSet == HiddenLevelSet) {
            if (direction > 0) {
                id = AreaData.Areas.FindLastIndex(area => area.LevelSet == currentLevelSet) + direction;
            } else {
                id = AreaData.Areas.FindIndex(area => area.LevelSet == currentLevelSet) + direction;
            }

            if (id >= AreaData.Areas.Count)
                id = 0;
            if (id < 0)
                id = AreaData.Areas.Count - 1;

            currentLevelSet = AreaData.Areas[id].LevelSet;
        }

        return id;
    }
    
    private static void modMapSearch(ILContext il) {
        ILCursor cursor = new(il);

        // target check: area.HasMode(AreaMode.Normal)
        // area is actually stored in a "DisplayClass" nested type, explaining the extra ldfld "area".
        if (cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloc(13),
            instr => instr.OpCode == OpCodes.Ldfld && (instr.Operand as FieldReference)!.Name == "area",
            instr => instr.MatchLdcI4(0),
            instr => instr.MatchCallvirt<AreaData>("HasMode"))) {

            // becomes: area.HasMode(AreaMode.Normal) && !IsCollabLevelSet(area.LevelSet)
            cursor.Emit(OpCodes.Ldloc_S, (byte) 13);
            cursor.Emit(OpCodes.Ldfld, (cursor.Instrs[cursor.Index - 4].Operand as FieldReference)!);
            cursor.EmitDelegate<Func<bool, AreaData, bool>>(HideScugHelper);
        }
    }

    private static void modMapListReloadItems(ILContext il) {
        ILCursor cursor = new(il);

        // target check: area.HasMode((AreaMode)side)
        // area is actually stored in a "DisplayClass" nested type, explaining the extra ldfld "area".
        if (cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloc(12),
            instr => instr.OpCode == OpCodes.Ldfld && (instr.Operand as FieldReference)!.Name == "area",
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<OuiMapList>("side"),
            instr => instr.MatchCallvirt<AreaData>("HasMode"))) {

            // becomes: area.HasMode((AreaMode)side) && !IsCollabLevelSet(area.LevelSet)
            cursor.Emit(OpCodes.Ldloc_S, (byte) 12);
            cursor.Emit(OpCodes.Ldfld, (cursor.Instrs[cursor.Index - 5].Operand as FieldReference)!);
            cursor.EmitDelegate<Func<bool, AreaData, bool>>(HideScugHelper);
        }
    }
    
    private static void modMapListCreateMenu(ILContext il) {
        ILCursor cursor = new(il);

        // target check: levelSet == "Celeste"
        if (cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdloc(1),
            instr => instr.MatchLdstr("Celeste"),
            instr => instr.MatchCall<string>("op_Equality"))) {

            // becomes: levelSet == "Celeste" || IsCollabLevelSet(levelSet)
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate<Func<bool, string, bool>>(HideScugHelper);
        }
    }
    
    
    private static void onSaveDataAfterInitialize(On.Celeste.SaveData.orig_AfterInitialize orig, SaveData self) {
        orig(self);

        if (self.CurrentSession_Safe == null || !self.CurrentSession_Safe.InArea) {
            if (!HideScugHelper(true, self.LastArea_Safe.LevelSet)) {
                self.LastArea_Safe = AreaData.Get("Celeste/0-Intro").ToKey();
            }
        }
    }
    
    private static void OnLevelOnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
        if (!HideScugHelper(true, level.Session.Area.GetSID()))
            SaveData.Instance.LastArea_Safe = AreaData.Get("Celeste/0-Intro").ToKey();
    }
    
    [Command("sid", "Shows the SID of the current map.")]
    internal static void ShowSIDCmd() {
        if (Engine.Scene is Level level)
            Engine.Commands.Log($"SID: {level.Session.Area.GetSID()}");
        else
            Engine.Commands.Log($"Not currently in a map.");
    }
}
