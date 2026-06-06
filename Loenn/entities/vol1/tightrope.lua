local utils = require("utils")
local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")

return {
    name = "ScugHelper/Tightrope",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {
                TieSprite = "objects/ScugHelper/hangrail/tie",
                RopeSprite = "objects/ScugHelper/hangrail/rope",
                width = 32
            },
        },
    },
    sprite = function(room, entity)
        local s = entity; local e = { x = entity.x + entity.width, y = entity.y }
        return {
            drawableFunc.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                    love.graphics.setColor({ 0.8, 0.7, 0.75, 1 })
                    love.graphics.line(s.x, s.y, e.x, e.y);
                end)
            end),
            drawableSprite.fromTexture(entity.TieSprite, s),
            drawableSprite.fromTexture(entity.TieSprite, e)
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(entity.x - 4, entity.y - 4, entity.width + 8, entity.height + 8)
    end
}
