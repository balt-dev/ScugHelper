using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Linq;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/CameraBlocker")]
public class CameraBlocker : Solid, IComparable
{
    public readonly string Flag;
    public readonly bool State;
    public readonly int Priority;
    public CameraBlocker(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false) {
        Collider = new Hitbox(data.Width, data.Height);
        Collidable = false;
        Flag = data.String("Flag");
        State = data.Bool("State");
        Priority = data.Int("Priority");
    }

    public override void Update() {
        base.Update();
    }

    [OnLoad] internal static void LoadHooks() => On.Celeste.Level.Update += OnLevelUpdate;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Level.Update -= OnLevelUpdate;

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        orig(self);
        CenterCameraTrigger.OnLevelUpdate(self);

        CameraBlocker[] blockers = [.. self.Tracker.GetEntities<CameraBlocker>().Select(e => e as CameraBlocker)];
        Array.Sort(blockers);
        foreach (CameraBlocker blocker in blockers) {
            if (blocker.Flag is not null && blocker.State != self.Session.GetFlag(blocker.Flag))
                continue;
            var cameraRect = new Rectangle((int)self.Camera.Left, (int)self.Camera.Top, (int)(self.Camera.Right - self.Camera.Left), (int)(self.Camera.Bottom - self.Camera.Top));
            if (!cameraRect.Intersects(blocker.Collider.Bounds))
                continue;

            var cameraPos = self.Camera.Position;

            Vector2 minOverlap = new(Math.Min(self.Camera.Right - blocker.Left, blocker.Right - self.Camera.Left), Math.Min(blocker.Bottom - self.Camera.Top, self.Camera.Bottom - blocker.Top));

            if (minOverlap.X < minOverlap.Y) {
                if (self.Camera.Left <= blocker.Right && self.Camera.Right >= blocker.Right)
                    cameraPos.X = Math.Max(self.Camera.Left, blocker.Right);
                else if (self.Camera.Right >= blocker.Left && self.Camera.Right <= blocker.Right)
                    cameraPos.X = Math.Min(self.Camera.Right, blocker.Left) - cameraRect.Width;
            } else {
                if (self.Camera.Top <= blocker.Bottom && self.Camera.Bottom >= blocker.Bottom)
                    cameraPos.Y = Math.Max(self.Camera.Top, blocker.Bottom);
                else if (self.Camera.Bottom >= blocker.Top && self.Camera.Top <= blocker.Top)
                    cameraPos.Y = Math.Min(self.Camera.Bottom, blocker.Top) - cameraRect.Height;
            }

            self.Camera.Position = cameraPos;
        }
    }

    public int CompareTo(object other) => Priority.CompareTo((other as CameraBlocker).Priority);
}
