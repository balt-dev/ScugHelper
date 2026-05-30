
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/ElsewhereWatchtower")]
public class ElsewhereLookout : Lookout {
    public readonly Vector2 StartPosition;
    public readonly string FlagWhileUsing;
    public ElsewhereLookout(EntityData data, Vector2 offset) : base(data, offset) {
        FlagWhileUsing = data.String("flagWhileUsing", "");
        var array = data.NodesOffset(offset)!.ToList();
        StartPosition = array[0];
        array.RemoveAt(0);
        nodes = array == null || array.Count == 0 ? null : [.. array];
    }

    static ILHook HookILLookoutLookRoutine;

    [OnLoad]
    internal static void LoadHooks() {
        HookILLookoutLookRoutine = new(typeof(Lookout).GetMethod(nameof(LookRoutine), BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), ILLookoutLookRoutine);
    }

    [OnUnload]
    internal static void UnloadHooks() {
        HookILLookoutLookRoutine?.Dispose();
    }

    static Vector2 SmuggledCameraStart;

    private static void ILLookoutLookRoutine(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, static match => match.MatchStfld<Lookout>(nameof(hud))))
            throw new Exception("Failed to hook for elsewhere watchtowers.");

        static void MoveCamera(Lookout self) {
            if (self is ElsewhereLookout look) {
                var camera = self.SceneAs<Level>().Camera;
                SmuggledCameraStart = camera.Position;
                new FadeWipe(self.Scene, wipeIn: false, () => {
                    camera.Position = look.StartPosition;
                    self.SceneAs<Level>().Session.SetFlag(look.FlagWhileUsing);
                    new FadeWipe(self.Scene, wipeIn: true).Duration = 0.2f;
                }).Duration = 0.2f;
            }
        }

        cur.EmitLdloc1();
        cur.EmitDelegate(MoveCamera);

        ILLabel label = null!;

        if (!cur.TryGotoNext(MoveType.After, static match => match.MatchLdstr("event:/ui/game/lookout_off")))
            throw new Exception("Failed to hook for elsewhere watchtowers.");
        
        if (!cur.TryGotoNextBestFit(MoveType.Before,
            static match => match.MatchLdloca(out _),
            static match => match.MatchCallOrCallvirt<Vector2>(nameof(Vector2.Length)),
            static match => match.MatchLdcR4(600),
            match => match.MatchBleUn(out label)
        )) throw new Exception("Failed to hook for elsewhere watchtowers.");

        cur.MoveAfterLabels();

        static bool MoveCameraBack(Lookout self) {
            if (self is ElsewhereLookout look) {
                var camera = self.SceneAs<Level>().Camera;
                var currentPos = camera.Position;
                var comp = new StaticCameraComponent(camera, currentPos);
                self.Add(comp);
                new FadeWipe(self.Scene, wipeIn: false, () => {
                    comp.RemoveSelf();
                    self.SceneAs<Level>().Session.SetFlag(look.FlagWhileUsing, false);
                    camera.Position = SmuggledCameraStart;
                    new FadeWipe(self.Scene, wipeIn: true).Duration = 0.2f;
                }).Duration = 0.2f;
                return true;
            }
            return false;
        }
        
        cur.EmitLdloc1();
        cur.EmitDelegate(MoveCameraBack);
        cur.EmitBrtrue(label);
    }

    private class StaticCameraComponent(Camera camera, Vector2 cameraPos) : Component(true, true)
    {
        public override void Update() {
            camera.Position = cameraPos;
            base.Update();
        }
        public override void Render() {
            camera.Position = cameraPos;
            base.Render();
        }
    }
}
