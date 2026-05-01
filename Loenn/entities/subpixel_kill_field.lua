local drawableFunc = require("structs.drawable_function")
local utils = require("utils")
local drawing = require("utils.drawing")

return {
    name = "ScugHelper/SubpixelKillField",
    depth = -100000,
    placements = {
        {
            name = "normal",
            data = {
                width = 8,
                height = 8,
                StartX = 0.5,
                StartY = 0.5,
                EndX = 0.5,
                EndY = 0.5
            }
        },
    },

    sprite = function(room, entity)
        return drawableFunc.fromFunction(function()
            drawing.callKeepOriginalColor(function()
                local left   = entity.x + math.min(math.max(entity.StartX, -0.5), 0.5)
                local top    = entity.y + math.min(math.max(entity.StartY, -0.5), 0.5)
                local width  = entity.width - math.min(math.max(-entity.EndX, -0.5), 0.5) - math.min(math.max(entity.StartX, -0.5), 0.5) - 1
                local height = entity.height - math.min(math.max(-entity.EndY, -0.5), 0.5) - math.min(math.max(entity.StartY, -0.5), 0.5) - 1
                love.graphics.setColor(1, 0, 0, 1)
                love.graphics.rectangle("line", left, top, width, height)
            end)
        end)
    end,
}
