local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/ControllableTempleFallController",
    depth = -1e20,
    placements = {
        {
            name = "normal",
            data = {}
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Controllable\nTemple Fall\nController",
                entity.x - 28, entity.y - 12, 56, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 28, entity.y - 12, 56, 24, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 28,
            entity.y - 12,
            56, 24
        )
    end
}
