local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/DeSeekerField",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32}
        },
    },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {122 / 255, 197 / 255, 51 / 255, 0.3}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, (entity.invert and {0, 0, 0, 0.5}) or {1, 1, 1, 0.5})
        }
    end,
}
