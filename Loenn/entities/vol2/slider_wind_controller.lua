local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/SliderWindController",
    depth = -1e20,
    placements = {
        {
            name = "normal",
            data = {
                SliderX = "WindStrengthX",
                SliderY = "WindStrengthY",
                BaseX = 0,
                BaseY = 0,
                Instant = false,
            }
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Slider Wind\nController",
                entity.x - 24, entity.y - 12, 48, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 24, entity.y - 12, 48, 24, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 24,
            entity.y - 12,
            48, 24
        )
    end
}
