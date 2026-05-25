
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
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
    public ElsewhereLookout(EntityData data, Vector2 offset) : base(data, offset) {
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

    private static void ILLookoutLookRoutine(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.After, static match => match.MatchStfld<Lookout>(nameof(hud))))
            throw new Exception("Failed to hook for elsewhere watchtowers.");

        static void MoveCamera(Lookout self) {
            if (self is ElsewhereLookout elsewhereLookout) {
                var camera = self.SceneAs<Level>().Camera;
                var delta = elsewhereLookout.StartPosition - camera.Position;
                if (delta.Length() > 600f) {
                    new FadeWipe(self.Scene, wipeIn: false, () => {
                        camera.Position = elsewhereLookout.StartPosition;
                        new FadeWipe(self.Scene, wipeIn: true).Duration = 0.2f;
                    }).Duration = 0.2f;
                } else {
                    static IEnumerator MoveCameraRoutine(ElsewhereLookout self) {
                        var camera = self.SceneAs<Level>().Camera;
                        var delta = self.StartPosition - camera.Position;
                        var was = camera.Position;
                        Vector2 direction = delta.SafeNormalize();
                        for (float duration = 0f; duration < 1f; duration += Engine.DeltaTime / 0.3f) {
                            camera.Position = was + (self.StartPosition - was) * Ease.CubeOut(Math.Clamp(duration, 0f, 1f));
                            yield return null;
                        }
                    }
                    self.Add(new Coroutine(MoveCameraRoutine(elsewhereLookout)));
                }
            }
        }

        cur.EmitLdloc1();
        cur.EmitDelegate(MoveCamera);
        
    }

}
