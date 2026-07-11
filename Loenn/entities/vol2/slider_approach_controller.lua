local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/SliderApproachController",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {
                TargetSlider = "DummySlider",
                DriverFlag = "ScugHelper.PlayerOnGround",
                DriverState = true,
                Value = 100,
                ApproachSpeed = 50
            }
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Slider Approach\nController",
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
    end,
    fieldOrder = {
        "x", "y", "Value", "ApproachSpeed", "DriverFlag", "TargetSlider", "DriverState"
    }
}
