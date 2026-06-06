local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/EagerSSVController",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { UpdateFrequency = 0.1 }
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Eager SSV\nController",
                entity.x - 27, entity.y - 12, 54, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 27, entity.y - 12, 54, 24, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 27,
            entity.y - 12,
            54, 24
        )
    end
}
