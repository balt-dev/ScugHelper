local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/LimboField",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32, invisible = false}
        },
    },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {0, 0, 0, 0.3}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, (entity.invert and {0, 0, 0, 0.5}) or {1, 1, 1, 0.5})
        }
    end,
}
