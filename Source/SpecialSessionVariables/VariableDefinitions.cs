using System;
using System.Linq;
using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
#nullable enable

namespace Celeste.Mod.ScugHelper.SpecialSessionVariables;

internal abstract class SpecialFlag() { public abstract bool GetValue(Level level); public virtual void SetValue(Level level, bool value) { } }
internal abstract class SpecialCounter() { public abstract int GetValue(Level level); public virtual void SetValue(Level level, int value) { } }
internal abstract class SpecialSlider() { public abstract float GetValue(Level level); public virtual void SetValue(Level level, float value) { } }

internal static class LevelExt
{
    internal static Player? GetPlayer(this Level level) => level.Tracker.GetEntity<Player>();
}

internal class PlayerDeadFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.Dead ?? true;
    public override void SetValue(Level level, bool value)
    {
        if (value && level.GetPlayer() is Player player && !player.Dead) player.Die(Vector2.Zero, true);
    }
}

internal class HasGoldenFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.Leader.Followers.Any(static (follower) => follower.Entity is Strawberry strawberry && strawberry.Golden && !strawberry.Winged) ?? false;
    public override void SetValue(Level level, bool value) { }
}

internal class RestartedFromGoldenFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.Session.RestartedFromGolden;
}

internal class StartedFromBeginningFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.Session.StartedFromBeginning;
}

internal class PlayerOnGroundFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.OnGround() ?? false;
}

internal class PlayerOnSafeGroundFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.OnSafeGround ?? false;
}

internal class PlayerDashAttackingFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.DashAttacking ?? false;
}

internal class SaveQuitDisabledFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.SaveQuitDisabled;
    public override void SetValue(Level level, bool value) => level.SaveQuitDisabled = value;
}

internal class IsPlayerSeekerFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.Get<PlayerSeekerComponent>() is not null;
    public override void SetValue(Level level, bool value)
    {
        if (level.GetPlayer() is not Player player) return;
        if (value && player.Get<PlayerSeekerComponent>() is null) player.Add(new PlayerSeekerComponent());
        else if (!value && player.Get<PlayerSeekerComponent>() is not null) player.Components.RemoveAll<PlayerSeekerComponent>();
    }
}
internal class DreamBlocksEnabledFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.Session.Inventory.DreamDash;
    public override void SetValue(Level level, bool value)
    {
        level.Session.Inventory.DreamDash = value;
        if (value) foreach (DreamBlock block in level.Tracker.GetEntities<DreamBlock>()) block.ActivateNoRoutine();
        else foreach (DreamBlock block in level.Tracker.GetEntities<DreamBlock>()) block.DeactivateNoRoutine();
    }
}
internal class HasMidairFlag : SpecialFlag
{
    public override bool GetValue(Level level) => MidairRefill.MidairDashCount > 0;
    public override void SetValue(Level level, bool value) => MidairRefill.MidairDashCount = value ? 1 : 0;
}
internal class HasOverchargeFlag : SpecialFlag
{
    public override bool GetValue(Level level) => OverchargeRefill.OverchargeDashCount > 0;
    public override void SetValue(Level level, bool value) => OverchargeRefill.OverchargeDashCount = value ? 1 : 0;
}
internal class HasLimboFlag : SpecialFlag
{
    public override bool GetValue(Level level) => LimboRefill.LimboTimer > 0;
    public override void SetValue(Level level, bool value) => LimboRefill.LimboTimer = value ? MathF.Max(LimboRefill.LimboTimer, LimboRefill.RefreshLimboLength) : 0;
}
internal class PlayerInvertedFlag : SpecialFlag
{
    public override bool GetValue(Level level) => GravityHelperImports.PlayerInverted();
    public override void SetValue(Level level, bool value) => GravityHelperImports.SetPlayerInverted(value);
}

internal class FrostHelperEnabledFlag : SpecialFlag
{
    public override bool GetValue(Level level) => FrostHelperImports.IsLoaded;
}
internal class GravityHelperEnabledFlag : SpecialFlag
{
    public override bool GetValue(Level level) => GravityHelperImports.IsLoaded;
}
internal class PlayerHoldingFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.Holding is not null;
}
internal class PlayerDuckingFlag : SpecialFlag
{
    public override bool GetValue(Level level) => level.GetPlayer()?.Ducking ?? false;
}

internal class PlayerStateCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.GetPlayer()?.StateMachine.State ?? -1;
    public override void SetValue(Level level, int value) { if (value >= 0 && level.GetPlayer() is Player player) player.StateMachine.State = value; }
}
internal class PlayerDashesCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.GetPlayer()?.Dashes ?? 0;
    public override void SetValue(Level level, int value) { if (level.GetPlayer() is Player player) player.Dashes = value; }
}
internal class PlayerMaxDashesCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Session.Inventory.Dashes;
    public override void SetValue(Level level, int value) => level.Session.Inventory.Dashes = value;
}
internal class PlayerTotalDashesCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Session.Dashes;
}

internal class LevelXCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Bounds.X;
}
internal class LevelYCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Bounds.Y;
}
internal class LevelWidthCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Bounds.Width;
}
internal class LevelHeightCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Bounds.Height;
}
internal class FPSCounter : SpecialCounter
{
    public override int GetValue(Level level) => Engine.FPS;
}
internal class DeathCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Session.Deaths;
}
internal class HereDeathCounter : SpecialCounter
{
    public override int GetValue(Level level) => level.Session.DeathsInCurrentLevel;
}
internal class CassetteBlockIndexCounter : SpecialCounter
{
    public override int GetValue(Level level)
    {
        if (level.Tracker.GetEntity<CassetteBlockManager>() is not CassetteBlockManager manager) return 0;
        try { return manager.GetSixteenthNote(); }
        catch (DivideByZeroException) { return 0; }
    }
}




internal class TimeRateSlider : SpecialSlider
{
    public override float GetValue(Level level) => Engine.EffectiveTimeRate;
}

internal class PlayerXSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.X ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is not Player player) return; bool oldNaive = player.TreatNaive; player.TreatNaive = true; player.MoveToX(value); player.TreatNaive = oldNaive; }
}
internal class PlayerYSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.Y ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is not Player player) return; bool oldNaive = player.TreatNaive; player.TreatNaive = true; player.MoveToY(value); player.TreatNaive = oldNaive; }
}
internal class PlayerSpeedXSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.Speed.X ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.Speed.X = value; }
}
internal class PlayerSpeedYSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.Speed.Y ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.Speed.Y = value; }
}
internal class PlayerSubpixelXSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.movementCounter.X ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.movementCounter.X = Math.Clamp(value, -0.5f, 0.5f); }
}
internal class PlayerSubpixelYSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.movementCounter.Y ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.movementCounter.Y = Math.Clamp(value, -0.5f, 0.5f); }
}
internal class PlayerStaminaSlider : SpecialSlider
{
    public override float GetValue(Level level) => level.GetPlayer()?.Stamina ?? 0f;
    public override void SetValue(Level level, float value) { if (level.GetPlayer() is Player player) player.Stamina = value; }
}
