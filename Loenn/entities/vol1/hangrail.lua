local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")

local populate = scughelper.populateDefaults {
    Sprite = "hangrail",
    TieSprite = "objects/ScugHelper/hangrail/tie",
    RopeSprite = "objects/ScugHelper/hangrail/rope",
    StartWithGravity = false,
    Position = 0,
    TakesStamina = true,
    PlayerMaxSpeed = 30,
    Friction = 80,
    MaxFall = 160,
    Gravity = 400,
    HoldSpeedLimit = 60,
    TiltSpriteThreshold = 20,
    StaminaCost = 12,
    JumpStaminaCost = 27.5,
    InitialSpeed = 0,
    NoBonk = false
}

return {
    name = "ScugHelper/Hangrail",
    depth = 100,
    nodeLineRenderType = "none",
    nodeLimits = { 2, 2 },
    placements = {
        {
            name = "normal",
            data = populate {},
        },
    },
    sprite = function(room, entity)
        entity.nodes = entity.nodes or {}
        entity.nodes[1] = entity.nodes[1] or { x = entity.x, y = entity.y }
        entity.nodes[2] = entity.nodes[2] or { x = entity.x, y = entity.y }
        local s = entity.nodes[1]; local e = entity.nodes[2]
        local bar = drawableSprite.fromTexture("objects/ScugHelper/hangrail/bar03", entity)
        bar.x = s.x + (e.x - s.x) * entity.Position
        bar.y = s.y + (e.y - s.y) * entity.Position
        return {
            drawableFunc.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                    love.graphics.setColor({ 0.8, 0.7, 0.75, 1 })
                    love.graphics.line(
                        s.x, s.y, e.x, e.y
                    );
                end)
            end),
            bar
        }
    end,
    selection = function(room, entity)
        entity.Position = entity.Position or 0
        entity.nodes = entity.nodes or {}
        entity.nodes[1] = entity.nodes[1] or { x = entity.x, y = entity.y }
        entity.nodes[2] = entity.nodes[2] or { x = entity.x, y = entity.y }
        local s = entity.nodes[1]; local e = entity.nodes[2]
        local bx = s.x + (e.x - s.x) * entity.Position
        local by = s.y + (e.y - s.y) * entity.Position
        return utils.rectangle(bx - 6, by - 6, 12, 12), {
            utils.rectangle(s.x - 2, s.y - 2, 4, 4),
            utils.rectangle(e.x - 2, e.y - 2, 4, 4),
        }
    end,
    nodeTexture = function(room, entity) return entity.TieSprite end,
    ignoredFields = function(entity)
        populate(entity)
        return {"_name", "_id", "originX", "originY"}
    end,
}
