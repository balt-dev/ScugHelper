
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[TrackedAs(typeof(DreamBlock))]
[CustomEntity("ScugHelper/DreamField")]
public class DreamField : DreamBlock {
    const float FieldOpacity = 0.3f;
    float Elapsed;

    internal class DreamFieldColliderList : ColliderList {
        public DreamFieldColliderList(Collider collider) => colliders = [collider];
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity)
            => entity is Player player && (player.StateMachine.State == Player.StDreamDash || (
                CommunalHelperInterop.DreamTunnelDashState is int StDreamTunnelDash &&
                player.StateMachine.State == StDreamTunnelDash
            ));
    }

    public DreamField(EntityData data, Vector2 offset) : base(data, offset) {
        Depth = 8400;
        Collider = new DreamFieldColliderList(Collider);
        Elapsed = 0f;
        DisableLightsInside = false;
    }

    public override void Update() {
        base.Update();
        Components.RemoveAll<LightOcclude>();
        if (playerHasDreamDash)
            Elapsed += Engine.DeltaTime;
    }

    public override void Render() {
        Camera camera = SceneAs<Level>().Camera;
        if (Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom)
            return;
        WobblyHelper.RenderFill(camera, Collider.Bounds, Elapsed, 2f, 2f, (playerHasDreamDash ? activeBackColor : disabledBackColor) * FieldOpacity);
        WobblyHelper.RenderFill(camera, Collider.Bounds.Grow(-2), Elapsed, 2f, 2f, (playerHasDreamDash ? activeBackColor : disabledBackColor) * FieldOpacity);
        DrawParticles();
        if (whiteFill > 0.01)
            WobblyHelper.RenderFill(
                camera,
                new Rectangle(
                    (int)Collider.AbsoluteLeft, (int)Collider.AbsoluteTop,
                    (int)Collider.Width, (int)(Collider.Height * whiteHeight)
                ),
                Elapsed, 2f, 2f, Color.White * whiteFill
            );
        else
            whiteHeight = 1;
    }

    private void DrawParticles() {
        Vector2 position = SceneAs<Level>().Camera.Position;
        for (int i = 0; i < particles.Length; i++) {
            int layer = particles[i].Layer;
            Vector2 position2 = particles[i].Position;
            position2 += position * (0.3f + 0.25f * layer);
            position2 = PutInside(position2);
            Color color = particles[i].Color;
            MTexture mTexture = layer switch {
                0 => particleTextures[3 - (int)((particles[i].TimeOffset * 4f + animTimer) % 4f)],
                1 => particleTextures[1 + (int)((particles[i].TimeOffset * 2f + animTimer) % 2f)],
                _ => particleTextures[2]
            };

            if (position2.X >= X + 2f && position2.Y >= Y + 2f && position2.X < Right - 2f && position2.Y < base.Bottom - 2f)
                mTexture.DrawCentered(position2 + shake, color);
        }
    }

    [OnLoad] internal static void LoadHooks() {
        On.Celeste.Player.DreamDashUpdate += OnPlayerDreamDashUpdate;
    }
    [OnUnload] internal static void UnloadHooks() {
        On.Celeste.Player.DreamDashUpdate -= OnPlayerDreamDashUpdate;
    }

    private static int OnPlayerDreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self) {
        if (self.dreamBlock is DreamField) {
            if (Input.Jump.Pressed) {
                Celeste.Freeze(0.05f);
                if (Math.Abs(Input.Aim.Value.X) > 0.01f) {
                    self.Speed.X = Math.Abs(self.Speed.X) * Math.Sign(Input.Aim.Value.X);
                    self.DashDir = Input.GetAimVector(self.Facing);
                }
                self.dreamJump = true;
                self.Jump();
                self.StateMachine.State = Player.StNormal;
                var sol = new Solid(Vector2.Zero, 0, 0, false);
                if (!self.TrySquishWiggle(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = self.Position }, 6, 6))
                    self.Die(Vector2.Zero, true);
                return 0;
            }
            if (Input.Grab.Check && self.Holding == null) {
                foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
                    if (component.Check(self) && self.Pickup(component)) {
                        Audio.Play("event:/char/madeline/crystaltheo_lift");
                        break;
                    }
            } else if (!Input.Grab.Check && self.Holding is {}) {
                if (Input.MoveY.Value == 1)
                    self.Drop();
                else {
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                    self.Holding.Release(Vector2.UnitX * (self.Speed.X == 0 ? (int) self.Facing : Math.Sign(self.Speed.X)));
                    self.Play("event:/char/madeline/crystaltheo_throw");
                }

                self.Holding = null;
            }
            if (Input.CrouchDashPressed || Input.Dash.Pressed) {
                bool demo = Input.CrouchDashPressed;
                Celeste.Freeze(0.05f);
                if (Math.Abs(Input.Aim.Value.X) > 0.01f) {
                    self.Speed.X = Math.Abs(self.Speed.X) * Math.Sign(Input.Aim.Value.X);
                    self.DashDir = Input.GetAimVector(self.Facing);
                }
                self.StateMachine.State = 2;
                var sol = new Solid(Vector2.Zero, 0, 0, false);
                if (!self.TrySquishWiggle(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = self.Position }, 6, 6))
                    self.Die(Vector2.Zero, true);
                self.Dashes--;
                int res = self.Holding?.Entity is RefillCrystal holdCrys ? holdCrys.UseCrystal(self) : self.StartDash();
                if (demo) {
                    self.Ducking = true;
                    self.demoDashed = true;
                }
                return res;
            }
        }
        return orig(self);
    }
}
