local utils = require("utils")
local drawing = require("utils.drawing")

return {
    name = "ScugHelper/TeleportGate",
    depth = 100,
    nodeLineRenderType = "line",
    nodeLimits = {1, 1},
    placements = {
        {
            name = "normal",
            data = {Angle = 0, Size = 32, Silent = false, Invisible = false, KeepX = false, KeepY = false, TeleportCamera = true},
        },
    },
    draw = function(room, entity)
        drawing.callKeepOriginalColor(function()
            local lineX = math.cos(entity.Angle * math.pi / 180)
            local lineY = math.sin(entity.Angle * math.pi / 180)
            love.graphics.setColor { 0, 1, 1 }
            love.graphics.line(
                entity.x - lineX * entity.Size / 2,
                entity.y - lineY * entity.Size / 2,
                entity.x + lineX * entity.Size / 2,
                entity.y + lineY * entity.Size / 2
            );
        end)
    end,
    nodeRectangle = function (room, entity, node, nodeIndex, viewport)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        return utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4)
    end,
    nodeFillColor = {0, 0, 0, 0},
    nodeBorderColor = {1, 1, 1, 1},
    selection = function(room, entity)
        local nodes = entity.nodes or {}
        local lineX = math.cos(entity.Angle * math.pi / 180)
        local lineY = math.sin(entity.Angle * math.pi / 180)
        local nodeX, nodeY = nodes[1].x or entity.x, nodes[1].y or entity.y
        return utils.rectangle(
            entity.x - lineX * entity.Size / 2,
            entity.y - lineY * entity.Size / 2,
            math.max(8, lineX * entity.Size),
            math.max(8, lineY * entity.Size)
        ), { utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4) }
    end
}
