local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/LevelEndController",
    placements = {
        name = "normal",
        data = {
            FlagToCheck = "",
            ShowCompleteScreen = true,
            ShowSpotlight = true,
            ScreenWipe = true,
        }
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Level End Controller",
                entity.x - 20, entity.y - 12, 40, 16, nil, 1
            ),
            drawableText.fromText(
                entity.FlagToCheck,
                entity.x - 20, entity.y + 4, 40, 8, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x - 20, entity.y - 12, 40, 24, {1, 1, 1})
        }
    end,
    selection = function(room, entity)
        local nodes = entity.nodes or {}
        local nodeX, nodeY = nodes[1].x or entity.x, nodes[1].y or entity.y
        return utils.rectangle(
            entity.x - 20,
            entity.y - 12,
            40, 24
        ), { utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4) }
    end,
}
