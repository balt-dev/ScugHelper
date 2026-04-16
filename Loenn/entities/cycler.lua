local utils = require("utils")
local drawing = require("utils.drawing")

return {
    name = "ScugHelper/Cycler",
    depth = 100,
    placements = {
        {
            name = "cycler",
            data = {Radius = 32, RPM = 30, Phase = 0, AttachedEntityID = 0, KeepX = false, KeepY = false}
        },
    },
    draw = function(room, entity)
        drawing.callKeepOriginalColor(function()
            local targetX = entity.x + math.cos(2 * math.pi * (entity.Phase or 0)) * (entity.Radius or 0);
            local targetY = entity.y + math.sin(2 * math.pi * (entity.Phase or 0)) * (entity.Radius or 0);
            love.graphics.setColor { 0.5, 0, 1 }
            love.graphics.circle("line", entity.x, entity.y, entity.Radius or 0);
            love.graphics.setColor { 0, 1, 0 }
            love.graphics.line(entity.x, entity.y, targetX, targetY)
            love.graphics.setColor { 1, 0, 0 }
            love.graphics.circle("line", entity.x, entity.y, 3);
            love.graphics.setColor { 1, 1, 0 }
            love.graphics.circle("line", targetX, targetY, 3);
        end)
    end,
    selection = function(room, entity)
        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    end,
}
