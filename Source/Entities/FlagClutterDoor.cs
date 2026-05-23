using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/FlagClutterDoor")]
public class FlagClutterDoor : ClutterDoor
{
    public static FlagClutterDoor Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new(entityData, offset, level.Session);

    public readonly string Flag;
    public readonly bool TargetState;
    bool flagStateLastUpdate;

    public FlagClutterDoor(EntityData data, Vector2 offset, Session session) : base(data, offset, session) {
        Collider = new Hitbox(32, 32);
        sprite.Position = new(Collider.Width / 2, Collider.Height / 2);
        Color = (ClutterBlock.Colors)(-1);
        Flag = data.String("Flag", "");
        TargetState = data.Bool("TargetState", true);
        flagStateLastUpdate = session.GetFlag(Flag);
        Collidable = flagStateLastUpdate != TargetState;
        if (Collidable) InstantLock(); else InstantUnlock();
    }

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.ClutterDoor.Update += OnUpdate;
        On.Celeste.ClutterDoor.IsLocked += OnIsLocked;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.ClutterDoor.Update -= OnUpdate;
        On.Celeste.ClutterDoor.IsLocked -= OnIsLocked;
    }

    private static bool OnIsLocked(On.Celeste.ClutterDoor.orig_IsLocked orig, ClutterDoor self, Session session)
        => self is FlagClutterDoor flagDoor ? session.GetFlag(flagDoor.Flag) != flagDoor.TargetState : orig(self, session);


    void LoudUnlock() {
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Audio.Play("event:/game/03_resort/forcefield_vanish", Position);
        sprite.Play("open");
        Collidable = false;
    }

    void InstantLock() {
        Visible = true;
        sprite.Play("idle");
        Collidable = true;
    }
    void Lock() {
        Visible = true;
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Audio.Play("event:/game/03_resort/forcefield_bump", Position);
        sprite.Play("idle");
        Collidable = true;
    }

    public override void Update() {
        base.Update();
        if (SceneAs<Level>().Session.GetFlag(Flag) == TargetState && flagStateLastUpdate != TargetState) LoudUnlock();
        else if (SceneAs<Level>().Session.GetFlag(Flag) != TargetState && flagStateLastUpdate == TargetState) Lock();
        flagStateLastUpdate = SceneAs<Level>().Session.GetFlag(Flag);
    }
    
    private static void OnUpdate(On.Celeste.ClutterDoor.orig_Update orig, ClutterDoor self) {
        orig(self);
        self.SurfaceSoundIndex = self.HasPlayerOnTop() ? 40 : 20; // Vanilla has no footsteps and I don't like that
    }
}
#nullable restore
