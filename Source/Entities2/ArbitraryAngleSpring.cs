using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/ArbitraryAngleSpring")]
public class ArbitraryAngleSpring : Spring {
    private const float MoveRadius = 8f;
    private const float PufferMoveRadius = 16f;
    public readonly Vector2 FacingDir;
    public readonly float BounceStrength;
    public readonly bool NoDashRefill;
    public readonly bool NoStaminaRefill;
    public readonly bool NoBackCollide;

    public ArbitraryAngleSpring(EntityData data, Vector2 offset) : base(data, offset, Orientations.Floor)
    {
        NoDashRefill = data.Bool("NoDashRefill", false);
        NoStaminaRefill = data.Bool("NoStaminaRefill", false);
        NoBackCollide = data.Bool("NoBackCollide", true);
        BounceStrength = data.Float("BounceStrength", 240f);
        FacingDir = Calc.AngleToVector((data.Float("Angle") - 90) * Calc.DegToRad, 1);
        staticMover.SolidChecker = (s) => CollideCheck(s, Position + Vector2.UnitX) || CollideCheck(s, Position - Vector2.UnitX) || CollideCheck(s, Position + Vector2.UnitY) || CollideCheck(s, Position - Vector2.UnitY);
        staticMover.JumpThruChecker = (jt) => CollideCheck(jt, Position + Vector2.UnitX) || CollideCheck(jt, Position - Vector2.UnitX) || CollideCheck(jt, Position + Vector2.UnitY) || CollideCheck(jt, Position - Vector2.UnitY);
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Components.RemoveAll<PlayerCollider>();
        Components.RemoveAll<HoldableCollider>();
        Components.RemoveAll<PufferCollider>();
        Add(new PlayerCollider(CustomOnCollide));
        Add(new HoldableCollider(CustomOnHoldable));
        Add(new PufferCollider(CustomOnPuffer));
        
        sprite.Path = data.String("SpritePath", "objects/spring") + "/";
        sprite.Rotation = FacingDir.Angle() + MathF.PI / 2;
    }

    private void CustomOnHoldable(Holdable holdable) {
        if (SpeedAccessor.For(holdable.Entity) is not SpeedAccessor spd) { OnHoldable(holdable); return; }
        Vector2 oldSpeed = spd.Speed;
        if (holdable.HitSpring(this)) {
            BounceAnimate();
            spd.Speed = MathF.Max(BounceStrength, oldSpeed.Length()) * FacingDir;
            holdable.SceneAs<Level>().DirectionalShake(FacingDir, 0.1f);
            holdable.Entity.X = MathF.Round(Position.X + FacingDir.X * holdable.Entity.Width * 2);
            holdable.Entity.Y = MathF.Round(Position.Y + FacingDir.Y * holdable.Entity.Height * 2);
        }
    }
    
    
    private void CustomOnPuffer(Puffer puffer) {
        Vector2 oldSpeed = puffer.hitSpeed;
        puffer.bounceWiggler.Start();
        puffer.Alert(restart: true, playSfx: false);
        BounceAnimate();
        puffer.hitSpeed = MathF.Max(BounceStrength, oldSpeed.Length()) * FacingDir * (puffer.IsInverted() ? new(1, -1) : Vector2.One);
        puffer.SceneAs<Level>().DirectionalShake(FacingDir, 0.1f);
        puffer.MoveToX(Position.X + FacingDir.X * PufferMoveRadius);
        puffer.MoveToY(Position.Y + FacingDir.Y * PufferMoveRadius);
    }

    public void CustomOnCollide(Player player)
    {
        if (player.StateMachine.State == 9 || !playerCanUse) return;
        if (NoBackCollide && Vector2.Dot(player.AdjustedSpeed().SafeNormalize(), FacingDir) > 0) return;

        BounceAnimate();
        
        player.SetAdjustedSpeed(MathF.Max(BounceStrength, player.AdjustedSpeed().Length()) * FacingDir);
        player.level.DirectionalShake(FacingDir, 0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        if ((player.LastBooster?.BoostingPlayer ?? false) && ((player.LastBooster is PinballBooster booster && booster.ConsumeBounce()) || ScugHelperModule.Settings.AllBoostersBounce)) {
            player.MoveToX(Position.X + FacingDir.X * MoveRadius);
            player.MoveToY(Position.Y + FacingDir.Y * MoveRadius);
            // Pinball booster shenanigans
            player.LastBooster.sprite.Scale = Vector2.One * ScugHelperModule.Settings.PinballBoosterSquash;
            return;
        }
        
        if (!player.Inventory.NoRefills && !NoDashRefill) player.RefillDash();
        if (!NoStaminaRefill) player.RefillStamina();
        player.StateMachine.State = 0;
        player.jumpGraceTimer = 0f;
        player.varJumpTimer = 0.2f;
        player.varJumpSpeed = player.Speed.Y;
        player.AutoJump = true;
        player.AutoJumpTimer = 0f;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
    }
}