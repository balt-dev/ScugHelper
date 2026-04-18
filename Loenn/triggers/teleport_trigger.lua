local utils = require("utils")

return {
    name = "ScugHelper/TeleportTrigger",
    depth = 100,
    nodeLineRenderType = "line",
    nodeLimits = {1, 1},
    placements = {
        {
            name = "trigger",
            data = {width = 32, height = 32, Silent = false, KeepX = false, KeepY = false, TeleportCamera = true, Flag = ""},
        },
    },
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1},
    nodeRectangle = function (room, entity)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        return utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4)
    end,
    nodeFillColor = {0, 0, 0, 0},
    nodeBorderColor = {1, 1, 1, 1}
}
