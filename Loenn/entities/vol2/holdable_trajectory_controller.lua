local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/HoldableTrajectoryController",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { MultiplierX = 1, MultiplierY = 1, InheritSpeedX = false, InheritSpeedY = false }
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Holdable Trajectory\nController",
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
