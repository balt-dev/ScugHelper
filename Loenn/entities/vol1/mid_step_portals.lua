local utils = require("utils")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/MidStepPortals",
    depth = -150000,
    placements = {
        {
            name = "normal",
            data = { width = 64, height = 64, Silent = false, Invisible = false, Orientation = 0, Offset = 0 }
        },
    },
    fieldInformation = {
        Orientation = { options = { Horizontal = 0, Vertical = 1 } },
        Offset = { fieldType = "integer" }
    },
    sprite = function(room, entity)
        if (entity.Orientation == 1) then
            return {
                drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, 1, { 0, 1, 0, 1 }),
                drawableRect.fromRectangle("fill", entity.x + entity.Offset, entity.y + entity.height - 1, entity.width,
                    1,
                    { 1, 0, 1, 1 })
            }
        else
            return {
                drawableRect.fromRectangle("fill", entity.x, entity.y, 1, entity.height, { 0, 1, 0, 1 }),
                drawableRect.fromRectangle("fill", entity.x + entity.width - 1, entity.y + entity.Offset, 1,
                    entity.height,
                    { 1, 0, 1, 1 })
            }
        end
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x + ((entity.Orientation == 1 and math.min(entity.Offset, 0) or 0)),
            entity.y + ((entity.Orientation == 0 and math.min(entity.Offset, 0)) or 0),
            entity.width + ((entity.Orientation == 1 and math.max(entity.Offset, 0) or 0)),
            entity.height + ((entity.Orientation == 0 and math.max(entity.Offset, 0) or 0))
        )
    end
}
