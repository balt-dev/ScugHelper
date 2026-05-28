#nullable enable

using System;

namespace Celeste.Mod.ScugHelper.SpecialSessionVariables;

internal static class ExtVarInterop
{
    internal static void LoadVariables()
    {
        Logger.Log(nameof(ScugHelperModule), "Loading Extended Variants variables!");
        SpecialSessionVariables.flags[$"ScugHelper.ExtendedVariantMode.Enabled"] = new ExtVarEnabledFlag();
        foreach (string name in (string[]) [
            "DashAssist",
            "Hiccups",
            "InfiniteStamina",
            "Invincible",
            "InvisibleMotion",
            "LowFriction",
            "MirrorMode",
            "NoGrabbing",
            "PlayAsBadeline",
            "SuperDashing",
            "ThreeSixtyDashing",
            "AffectExistingChasers",
            "AllStrawberriesAreGoldens",
            "AllowLeavingTheoBehind",
            "AllowThrowingTheoOffscreen",
            "AlternativeBuffering",
            "AlwaysFeather",
            "AlwaysInvisible",
            "AutoDash",
            "AutoJump",
            "BadelineBossesEverywhere",
            "BadelineChasersEverywhere",
            "BounceEverywhere",
            "BufferableGrab",
            "ChangePatternsOfExistingBosses",
            "ConsistentThrowing",
            "CornerboostProtection",
            "CorrectedMirrorMode",
            "CrouchDashFix",
            "DashBeforePickup",
            "DashTrailAllTheTime",
            "DisableClimbJumping",
            "DisableDashCooldown",
            "DisableAutoJumpGravityLowering",
            "DisableJumpGravityLowering",
            "DisableJumpingOutOfWater",
            "DisableKeysSpotlight",
            "DisableMadelineSpotlight",
            "DisableNeutralJumping",
            "DisableOshiroSlowdown",
            "DisableRefillsOnScreenTransition",
            "DisableSeekerSlowdown",
            "DisableSuperBoosts",
            "DisableWallJumping",
            "DisplayDashCount",
            "DontRefillStaminaOnGround",
            "EveryJumpIsUltra",
            "EverythingIsUnderwater",
            "FirstBadelineSpawnRandom",
            "ForceDuckOnGround",
            "FriendlyBadelineFollower",
            "HeldDash",
            "InvertDashes",
            "InvertGrab",
            "InvertHorizontalControls",
            "InvertVerticalControls",
            "LegacyDashSpeedBehavior",
            "LiftboostProtection",
            "MidairTech",
            "NoFreezeFrames",
            "NoFreezeFramesAdvanceCassetteBlocks",
            "OshiroEverywhere",
            "PermanentBinoStorage",
            "PermanentDashAttack",
            "PreserveExtraDashesUnderwater",
            "PreserveWallbounceSpeed",
            "RefillJumpsOnDashRefill",
            "ResetJumpCountOnGround",
            "RestoreDashesOnRespawn",
            "RisingLavaEverywhere",
            "SaferDiagonalSmuggle",
            "SnowballsEverywhere",
            "StretchUpDashes",
            "TheoCrystalsEverywhere",
            "ThrowIgnoresForcedMove",
            "TrueNoGrabbing",
            "UltraProtection",
            "UpsideDown",
            "WalllessWallbounce",
            "WindCrouchMove"
        ]) SpecialSessionVariables.flags[$"ScugHelper.ExtendedVariantMode.{name}"] = new BooleanExtVarFlag(name);
        foreach (string name in (string[]) [
            "VanillaGameSpeed",
            "JumpCount",
            "AddSeekers",
            "BadelineBossCount",
            "BadelineBossNodeCount",
            "ChaserCount",
            "CornerCorrection",
            "DashCount",
            "JellyfishEverywhere",
            "MultiBuffering",
            "OshiroCount",
            "ScreenTransitionDashCount",
            "SpawnDashCount",
            "Stamina",
            "WallBounceDistance",
            "WallJumpDistance"
        ]) SpecialSessionVariables.counters[$"ScugHelper.ExtendedVariantMode.{name}"] = new IntExtVarCounter(name);
        foreach (string name in (string[]) [
            "AirFriction",
            "AnxietyEffect",
            "BackgroundBlurLevel",
            "BackgroundBrightness",
            "BadelineLag",
            "BlurLevel",
            "BoostMultiplier",
            "ClimbDownSpeed",
            "ClimbHoldStaminaDrainRate",
            "ClimbJumpStaminaCost",
            "ClimbUpSpeed",
            "ClimbUpStaminaDrainRate",
            "CoyoteTime",
            "DashLength",
            "DashSpeed",
            "DashTimerMultiplier",
            "DelayBeforeRegrabbing",
            "DelayBetweenBadelines",
            "ExplodeLaunchSpeed",
            "FallSpeed",
            "FastFallAcceleration",
            "ForegroundEffectOpacity",
            "Friction",
            "GameSpeed",
            "GlitchEffect",
            "Gravity",
            "HiccupStrength",
            "HorizontalSpringBounceDuration",
            "HorizontalWallJumpDuration",
            "HyperdashSpeed",
            "JumpBoost",
            "JumpCooldown",
            "JumpDuration",
            "JumpHeight",
            "LiftboostCapDown",
            "LiftboostCapUp",
            "LiftboostCapX",
            "MinimumDelayBeforeThrowing",
            "PickupDuration",
            "RegularHiccups",
            "RisingLavaSpeed",
            "RoomLighting",
            "RoomBloom",
            "ScreenShakeIntensity",
            "SlowfallGravityMultiplier",
            "SlowfallSpeedThreshold",
            "SnowballDelay",
            "SpeedX",
            "SuperdashSteeringSpeed",
            "UltraSpeedMultiplier",
            "UnderwaterSpeedX",
            "UnderwaterSpeedY",
            "WallBouncingSpeed",
            "WallSlidingSpeed",
            "WaterSurfaceSpeedX",
            "WaterSurfaceSpeedY",
            "ZoomLevel"
        ]) SpecialSessionVariables.sliders[$"ScugHelper.ExtendedVariantMode.{name}"] = new FloatExtVarSlider(name);
    }

    private class BooleanExtVarFlag(string name) : SpecialFlag {
        public override bool GetValue(Level level) => ExtendedVariantModeImports.GetBoolVariant(name);
        public override void SetValue(Level level, bool value) => ExtendedVariantModeImports.SetBoolVariant(name, value);
    }

    private class IntExtVarCounter(string name) : SpecialCounter {
        public override int GetValue(Level level) => ExtendedVariantModeImports.GetIntVariant(name);
        public override void SetValue(Level level, int value) => ExtendedVariantModeImports.SetIntVariant(name, value);
    }

    private class FloatExtVarSlider(string name) : SpecialSlider {
        public override float GetValue(Level level) { Logger.Log(nameof(ScugHelperModule), name); return ExtendedVariantModeImports.GetFloatVariant(name); }
        public override void SetValue(Level level, float value) => ExtendedVariantModeImports.SetFloatVariant(name, value);
    }
    
    internal class ExtVarEnabledFlag : SpecialFlag {
        public override bool GetValue(Level level) => ExtendedVariantModeImports.IsLoaded;
    }
}