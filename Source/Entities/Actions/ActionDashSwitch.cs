using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/ActionDashSwitch")]
public class ActionDashSwitch(EntityData data, Vector2 offset, EntityID id) : DashSwitch(data.Position + offset, data.Enum("side", Sides.Up), data.Bool("persistent"), false, id, data.Attr("sprite", "default"))
{
    readonly string[] Targets = IAction.GetTargets(data);
    
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.DashSwitch.OnDashed += OnDashed;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.DashSwitch.OnDashed -= OnDashed;
    }
    
    private static DashCollisionResults OnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Vector2 direction) {
        if (self is ActionDashSwitch actionSwitch) {
            if (!self.pressed && direction == self.pressDirection) {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", self.Position);
                self.sprite.Play("push");
                self.pressed = true;
                self.MoveTo(self.pressedTarget);
                self.Collidable = false;
                self.Position -= self.pressDirection * 2f;
                self.SceneAs<Level>().ParticlesFG.Emit(self.mirrorMode ? P_PressAMirror : P_PressA, 10, self.Position + self.sprite.Position, direction.Perpendicular() * 6f, self.sprite.Rotation - MathF.PI);
                self.SceneAs<Level>().ParticlesFG.Emit(self.mirrorMode ? P_PressBMirror : P_PressB, 4, self.Position + self.sprite.Position, direction.Perpendicular() * 6f, self.sprite.Rotation - MathF.PI);
                ActionManager.AlertActions(actionSwitch.Targets, self.SceneAs<Level>());
                if (self.persistent)
                    self.SceneAs<Level>().Session.SetFlag(self.FlagName);
            }
    
            return DashCollisionResults.NormalCollision;
        } else return orig(self, player, direction);
    }
    
}
#nullable restore
