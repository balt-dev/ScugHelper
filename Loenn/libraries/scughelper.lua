local drawing = require("utils.drawing")
local utils = require("utils")
local drawableFunc = require("structs.drawable_function")

local T = {}

local builtins = {
    "#BerryCollect",
    "#CassetteBlock0",
    "#CassetteBlock1",
    "#CassetteBlock2",
    "#CassetteBlock3",
    "#PlayerAirborne",
    "#PlayerLand",
    "#PlayerDie",
    "#PlayerDash",
    "#PlayerGrab",
    "#PlayerClimbJump",
    "#PlayerSuperWallJump",
    "#PlayerSuperJump",
    "#PlayerWallJump",
    "#PlayerJump",
    "#PlayerSideflip",
    "#SeekerDie",
    "#InitActions",
    "#LoadLevel",
    "#Tick",
    "#DashSwitchHit",
    "#TouchSwitchActivated",
    "#TouchSwitchFinished",
    "#TorchLit",
    "#PlayerBounce",
    "#PlayerSuperBounce",
    "#PlayerPickup",
    "#PlayerDrop",
    "#PlayerPointBounce",
    "#PlayerThrow",
    "#PlayerRebound",
    "#PlayerReflectBounce",
    "#JellyfishFizzle",
}
T.actionGroups = { options = {}, searchable = true, editable = true }
for _, val in ipairs(builtins) do
    T.actionGroups.options[val] = val
end

local builtinFlags = {
    "ScugHelper.PlayerDead",
    "ScugHelper.HasGolden",
    "ScugHelper.RestartedFromGolden",
    "ScugHelper.StartedFromBeginning",
    "ScugHelper.PlayerOnGround",
    "ScugHelper.PlayerOnSafeGround",
    "ScugHelper.PlayerDashAttacking",
    "ScugHelper.IsPlayerSeeker",
    "ScugHelper.DreamBlocksEnabled",
    "ScugHelper.HasMidair",
    "ScugHelper.HasOvercharge",
    "ScugHelper.InLimbo",
    "ScugHelper.SaveQuitDisabled",
    "ScugHelper.PlayerHolding",
    "ScugHelper.InBooster",
    "ScugHelper.GravityHelper.Enabled",
    "ScugHelper.FrostHelper.Enabled",
    "ScugHelper.GravityHelper.PlayerInverted",
    "ScugHelper.ExtendedVariantMode.DashAssist",
    "ScugHelper.ExtendedVariantMode.Hiccups",
    "ScugHelper.ExtendedVariantMode.InfiniteStamina",
    "ScugHelper.ExtendedVariantMode.Invincible",
    "ScugHelper.ExtendedVariantMode.InvisibleMotion",
    "ScugHelper.ExtendedVariantMode.LowFriction",
    "ScugHelper.ExtendedVariantMode.MirrorMode",
    "ScugHelper.ExtendedVariantMode.NoGrabbing",
    "ScugHelper.ExtendedVariantMode.PlayAsBadeline",
    "ScugHelper.ExtendedVariantMode.SuperDashing",
    "ScugHelper.ExtendedVariantMode.ThreeSixtyDashing",
    "ScugHelper.ExtendedVariantMode.AffectExistingChasers",
    "ScugHelper.ExtendedVariantMode.AllStrawberriesAreGoldens",
    "ScugHelper.ExtendedVariantMode.AllowLeavingTheoBehind",
    "ScugHelper.ExtendedVariantMode.AllowThrowingTheoOffscreen",
    "ScugHelper.ExtendedVariantMode.AlternativeBuffering",
    "ScugHelper.ExtendedVariantMode.AlwaysFeather",
    "ScugHelper.ExtendedVariantMode.AlwaysInvisible",
    "ScugHelper.ExtendedVariantMode.AutoDash",
    "ScugHelper.ExtendedVariantMode.AutoJump",
    "ScugHelper.ExtendedVariantMode.BadelineBossesEverywhere",
    "ScugHelper.ExtendedVariantMode.BadelineChasersEverywhere",
    "ScugHelper.ExtendedVariantMode.BounceEverywhere",
    "ScugHelper.ExtendedVariantMode.BufferableGrab",
    "ScugHelper.ExtendedVariantMode.ChangePatternsOfExistingBosses",
    "ScugHelper.ExtendedVariantMode.ConsistentThrowing",
    "ScugHelper.ExtendedVariantMode.CornerboostProtection",
    "ScugHelper.ExtendedVariantMode.CorrectedMirrorMode",
    "ScugHelper.ExtendedVariantMode.CrouchDashFix",
    "ScugHelper.ExtendedVariantMode.DashBeforePickup",
    "ScugHelper.ExtendedVariantMode.DashTrailAllTheTime",
    "ScugHelper.ExtendedVariantMode.DisableClimbJumping",
    "ScugHelper.ExtendedVariantMode.DisableDashCooldown",
    "ScugHelper.ExtendedVariantMode.DisableAutoJumpGravityLowering",
    "ScugHelper.ExtendedVariantMode.DisableJumpGravityLowering",
    "ScugHelper.ExtendedVariantMode.DisableJumpingOutOfWater",
    "ScugHelper.ExtendedVariantMode.DisableKeysSpotlight",
    "ScugHelper.ExtendedVariantMode.DisableMadelineSpotlight",
    "ScugHelper.ExtendedVariantMode.DisableNeutralJumping",
    "ScugHelper.ExtendedVariantMode.DisableOshiroSlowdown",
    "ScugHelper.ExtendedVariantMode.DisableRefillsOnScreenTransition",
    "ScugHelper.ExtendedVariantMode.DisableSeekerSlowdown",
    "ScugHelper.ExtendedVariantMode.DisableSuperBoosts",
    "ScugHelper.ExtendedVariantMode.DisableWallJumping",
    "ScugHelper.ExtendedVariantMode.DisplayDashCount",
    "ScugHelper.ExtendedVariantMode.DontRefillStaminaOnGround",
    "ScugHelper.ExtendedVariantMode.EveryJumpIsUltra",
    "ScugHelper.ExtendedVariantMode.EverythingIsUnderwater",
    "ScugHelper.ExtendedVariantMode.FirstBadelineSpawnRandom",
    "ScugHelper.ExtendedVariantMode.ForceDuckOnGround",
    "ScugHelper.ExtendedVariantMode.FriendlyBadelineFollower",
    "ScugHelper.ExtendedVariantMode.HeldDash",
    "ScugHelper.ExtendedVariantMode.InvertDashes",
    "ScugHelper.ExtendedVariantMode.InvertGrab",
    "ScugHelper.ExtendedVariantMode.InvertHorizontalControls",
    "ScugHelper.ExtendedVariantMode.InvertVerticalControls",
    "ScugHelper.ExtendedVariantMode.LegacyDashSpeedBehavior",
    "ScugHelper.ExtendedVariantMode.LiftboostProtection",
    "ScugHelper.ExtendedVariantMode.MidairTech",
    "ScugHelper.ExtendedVariantMode.NoFreezeFrames",
    "ScugHelper.ExtendedVariantMode.NoFreezeFramesAdvanceCassetteBlocks",
    "ScugHelper.ExtendedVariantMode.OshiroEverywhere",
    "ScugHelper.ExtendedVariantMode.PermanentBinoStorage",
    "ScugHelper.ExtendedVariantMode.PermanentDashAttack",
    "ScugHelper.ExtendedVariantMode.PreserveExtraDashesUnderwater",
    "ScugHelper.ExtendedVariantMode.PreserveWallbounceSpeed",
    "ScugHelper.ExtendedVariantMode.RefillJumpsOnDashRefill",
    "ScugHelper.ExtendedVariantMode.ResetJumpCountOnGround",
    "ScugHelper.ExtendedVariantMode.RestoreDashesOnRespawn",
    "ScugHelper.ExtendedVariantMode.RisingLavaEverywhere",
    "ScugHelper.ExtendedVariantMode.SaferDiagonalSmuggle",
    "ScugHelper.ExtendedVariantMode.SnowballsEverywhere",
    "ScugHelper.ExtendedVariantMode.StretchUpDashes",
    "ScugHelper.ExtendedVariantMode.TheoCrystalsEverywhere",
    "ScugHelper.ExtendedVariantMode.ThrowIgnoresForcedMove",
    "ScugHelper.ExtendedVariantMode.TrueNoGrabbing",
    "ScugHelper.ExtendedVariantMode.UltraProtection",
    "ScugHelper.ExtendedVariantMode.UpsideDown",
    "ScugHelper.ExtendedVariantMode.WalllessWallbounce",
    "ScugHelper.ExtendedVariantMode.WindCrouchMove",
}
T.builtinFlags = { options = {}, searchable = true, editable = true }
for _, val in ipairs(builtinFlags) do
    T.builtinFlags.options[val] = val
end

function T.drawableGate(x, y, angle, size, color)
    return drawableFunc.fromFunction(function()
        drawing.callKeepOriginalColor(function()
            local lineX = math.cos(angle * math.pi / 180)
            local lineY = math.sin(angle * math.pi / 180)
            love.graphics.setColor(color)
            love.graphics.line(
                x - lineX * size / 2,
                y - lineY * size / 2,
                x + lineX * size / 2,
                y + lineY * size / 2
            );
        end)
    end)
end

function T.parseColor(color)
    local success, r, g, b = utils.parseHexColor(color)
    return success and { r, g, b } or { 0, 1, 1 }
end

T.colors = function(name)
    return ({
        playerAction = { 1, 0.5, 0.5, 1 },
        expressionAction = { 0.5, 1, 1, 1 },
        luaAction = { 0.1, 0.1, 1, 1 },
        metaAction = { 1, 1, 1, 1 },
    })[name]
end

T.playerStates = {
    Normal = 0,
    Climb = 1,
    Dash = 2,
    Swim = 3,
    Boost = 4,
    RedDash = 5,
    HitSquash = 6,
    Launch = 7,
    Pickup = 8,
    DreamDash = 9,
    SummitLaunch = 10,
    Dummy = 11,
    IntroWalk = 12,
    IntroJump = 13,
    IntroRespawn = 14,
    IntroWakeUp = 15,
    BirdDashTutorial = 16,
    Frozen = 17,
    ReflectionFall = 18,
    StarFly = 19,
    TempleFall = 20,
    CassetteFly = 21,
    Attract = 22,
    IntroMoonJump = 23,
    FlingBird = 24,
    IntroThinkForABit = 25
}

T.playerStateNames = {}
for state, id in pairs(T.playerStates) do
    T.playerStateNames[id] = state
end

T.populateDefaults = function(source)
    return function(destination)
        for key, value in pairs(source) do
            if destination[key] == nil then
                destination[key] = value
            end
        end
        return destination
    end
end

T.unfuckedRect = function(entity, color)
    return drawableFunc.fromFunction(function()
        drawing.callKeepOriginalColor(function()
            love.graphics.setColor(color)
            love.graphics.rectangle("line",
                entity.x,
                entity.y,
                entity.width,
                entity.height
            )
        end)
    end)
end

return T
