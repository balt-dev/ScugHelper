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
                StartX = -0.25,
                StartY = -0.25,
                EndX = 0.25,
                EndY = 0.25
            }
        },
    },
    fieldOrder = {"x", "y", "width", "height", "StartX", "StartY", "EndX", "EndY"},
    sprite = function(room, entity)
        return drawableFunc.fromFunction(function()
            drawing.callKeepOriginalColor(function()
                love.graphics.setColor(1, 0.2, 0.2, 1)
                love.graphics.rectangle("fill", entity.x, entity.y, entity.width, entity.height)
                love.graphics.setColor(0.2, 1, 1, 1)
                for x = 0.5, entity.width, 1 do
                    for y = 0.5, entity.height, 1 do
                        love.graphics.rectangle("fill", entity.x + entity.StartX + x, entity.y + entity.StartY + y, entity.EndX - entity.StartX, entity.EndY - entity.StartY)
                    end
                end
            end)
        end)
    end,
}
