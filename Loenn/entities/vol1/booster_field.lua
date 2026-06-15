local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/BoosterField",
    depth = function(room, entity) return entity.Depth or -20000 end,
    placements = {
        {
            name = "boosterfield",
            data = { width = 32, height = 32, invisible = false, invert = false, BounceLimit = -1, destroy = false, Depth = -20000 }
        },
    },
    fieldInformation = { Depth = {fieldType = "integer"} },
    sprite = function(room, entity)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, { 0.6, 0.2, 0.2, 0.3 }),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height,
                (entity.invert and { 0, 0, 0, 0.5 }) or { 1, 1, 1, 0.5 })
        }
    end,
}
