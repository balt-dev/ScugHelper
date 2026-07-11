local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/DiscreteMovementController",
    depth = -1e20,
    placements = {
        {
            name = "normal",
            data = {Steps = 16}
        },
    },
    fieldInformation = { Steps = { fieldType = "integer" } },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Discrete\nMovement\nController",
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
