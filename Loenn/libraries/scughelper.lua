local drawing = require("utils.drawing")
local utils = require("utils")
local drawableFunc = require("structs.drawable_function")

local T = {}

local builtins = {
    "#BerryCollect",
    "#CassetteBlock1",
    "#CassetteBlock2",
    "#CassetteBlock3",
    "#CassetteBlock4",
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
    "#SeekerDie",
    "#InitActions",
    "#LoadLevel",
    "#Tick",
    "#DashSwitchHit",
    "#TouchSwitchActivated",
    "#TouchSwitchFinished",
    "#TorchLit"
}
T.actionGroups = {options = {}, searchable = true, editable = true}
for _, val in ipairs(builtins) do
    T.actionGroups.options[val] = val
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
    return success and {r, g, b} or {1, 1, 1}
end

T.colors = function(name)
    return ({
        playerAction = { 1, 0.5, 0.5, 1 },
        expressionAction = { 0.5, 1, 1, 1 },
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
