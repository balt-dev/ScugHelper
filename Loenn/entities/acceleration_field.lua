local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/AccelerationField",
    depth = -10000,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32, AccelX = 0, AccelY = -60, Drag = 0.05, Everywhere = false}
        },
    },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {173 / 255, 216 / 255, 230 / 255, 0.1}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {240 / 255, 248 / 255, 1, 0.2})
        }
    end,
}
