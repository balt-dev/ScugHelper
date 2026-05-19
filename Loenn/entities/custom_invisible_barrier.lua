local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/CustomInvisibleBarrier",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { width = 32, height = 32, SurfaceSoundIndex = 33, Flag = "", FlagState = false, Climbable = false }
        },
    },
    fieldInformation = { SurfaceSoundIndex = { fieldType = "integer" } },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, { 1, 1, 1, 0.2 }),
        }
    end,
}
