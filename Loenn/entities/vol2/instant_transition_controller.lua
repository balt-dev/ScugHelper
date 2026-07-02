local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/InstantTransitionController",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { }
        },
    },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x - 27, entity.y - 12, 54, 5, { 0.8, 0.8, 1, 0.2 }),
            drawableRect.fromRectangle("fill", entity.x - 27, entity.y - 7, 54, 5, { 1, 0.8, 0.8, 0.2 }),
            drawableRect.fromRectangle("fill", entity.x - 27, entity.y - 2, 54, 4, { 1, 1, 1, 0.2 }),
            drawableRect.fromRectangle("fill", entity.x - 27, entity.y + 2, 54, 5, { 1, 0.8, 0.8, 0.2 }),
            drawableRect.fromRectangle("fill", entity.x - 27, entity.y + 7, 54, 5, { 0.8, 0.8, 1, 0.2 }),
            drawableText.fromText(
                "Instant Transition\nController",
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
