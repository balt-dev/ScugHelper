local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/StarjumpSpinnerColorController",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { Color = "FFFFFF", Rainbow = false, Bloom = false }
        },
    },
    fieldInformation = { Color = { fieldType = "color" } },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Outline Spinner\nColor Controller",
                entity.x - 36, entity.y - 12, 72, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 36, entity.y - 12, 72, 24, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 36,
            entity.y - 12,
            72, 24
        )
    end
}
