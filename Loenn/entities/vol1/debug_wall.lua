local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/DebugWall",
    depth = function(room, entity) return (entity.Background and 9999) or -9999 end,
    placements = {
        {
            name = "normal",
            data = { width = 32, height = 32, Background = false }
        },
    },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, { 1, 1, 1, 0.3 })
        }
    end
}
