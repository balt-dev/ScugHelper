local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/EditorHideController",
    depth = -1000000,
    placements = {
        {
            name = "normal",
            data = {},
        },
    },
    fieldInformation = {},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Debug Hide\nController",
                entity.x - 24, entity.y - 8, 48, 16, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 24, entity.y - 8, 48, 16, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        local nodes = entity.nodes or {}
        local nodeX, nodeY = nodes[1].x or entity.x, nodes[1].y or entity.y
        return utils.rectangle(
            entity.x - 24,
            entity.y - 8,
            48, 16
        )
    end
}
