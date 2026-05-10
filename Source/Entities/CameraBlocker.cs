using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/CameraBlocker")]
public class CameraBlocker : Solid
{
    public readonly string Flag;
    public readonly bool State;
    public CameraBlocker(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Collidable = false;
        Flag = data.String("Flag");
        State = data.Bool("State");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (scene is not Level level) { RemoveSelf(); return; }
        if (level.Tracker.GetEntity<CameraBox>() is null)
            level.Add(new CameraBox(level.Camera));
    }

    [OnLoad] internal static void LoadHooks() => On.Celeste.Level.Update += OnLevelUpdate;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Level.Update -= OnLevelUpdate;

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (self.Tracker.GetEntity<CameraBox>() is CameraBox box)
        {
            box.Position = self.Camera.Position;
            float oldWidth = box.Width;
            float oldHeight = box.Height;
            box.Collider = new CameraBox.CameraBoxColliderList(new Hitbox(self.Camera.Right - self.Camera.Left, self.Camera.Bottom - self.Camera.Top));
            orig(self);
            var deltaPos = self.Camera.Position - box.ExactPosition;
            foreach (CameraBlocker blocker in self.Tracker.GetEntities<CameraBlocker>()) blocker.Collidable = blocker.Flag is null || self.Session.GetFlag(blocker.Flag) == blocker.State;
            try
            { // fuck it
                box.MoveH(deltaPos.X);
                box.MoveV(deltaPos.Y);
            }
            catch (NullReferenceException) { }
            foreach (CameraBlocker blocker in self.Tracker.GetEntities<CameraBlocker>()) blocker.Collidable = false;
            self.Camera.Position = box.ExactPosition;
        }
        else orig(self);
    }

    [Tracked]
    private class CameraBox : Actor
    {
        public CameraBox(Camera camera) : base(camera.Position)
        {
            Collider = new CameraBoxColliderList(new Hitbox(camera.Right - camera.Left, camera.Bottom - camera.Top));
        }

        internal class CameraBoxColliderList : ColliderList
        {
            public CameraBoxColliderList(Hitbox hitbox) => colliders = [hitbox];

            public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
            public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
            public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
            public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

            private bool CheckEntity(Entity entity) => entity is CameraBlocker;
        }
    }
}
