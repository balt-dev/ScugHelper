local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
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
            data = {Angle = 0, Size = 32, Silent = false, Invisible = false, KeepX = false, KeepY = false, TeleportCamera = true, Flag = "", FlipFacing = false},
        },
    },
    fieldInformation = { Flag = scughelper.builtinFlags },
    sprite = function(room, entity)
        return scughelper.drawableGate(entity.x, entity.y, entity.Angle, entity.Size, {0, 1, 1})
    end,
    selection = function(room, entity)
        local nodes = entity.nodes or {}
        local nodeX, nodeY = nodes[1].x or entity.x, nodes[1].y or entity.y
        return utils.rectangle(
            entity.x - 8,
            entity.y - 8,
            16, 16
        ), { utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4) }
    end,
    nodeRectangle = function (room, entity, node, nodeIndex, viewport)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        return utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4)
    end,
    nodeFillColor = {0, 0, 0, 0},
    nodeBorderColor = {1, 1, 1, 1}
}
