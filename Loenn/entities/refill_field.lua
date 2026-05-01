local drawableRect = require("structs.drawable_rectangle")

local refill_colors = {
    [0] = {0, 0, 1},
    [1] = {0xa5 / 0xff, 0xad / 0xff, 0xff / 0xff},
    [2] = {0, 0, 0}
}

return {
    name = "ScugHelper/RefillField",
    depth = 100,
    placements = {
        { name = "midairOn", data = {width = 32, height = 32, RefillType = 0, State = true} },
        { name = "midairOff", data = {width = 32, height = 32, RefillType = 0, State = false} },
        { name = "overchargeOn", data = {width = 32, height = 32, RefillType = 1, State = true} },
        { name = "overchargeOff", data = {width = 32, height = 32, RefillType = 1, State = false} },
        { name = "limboOn", data = {width = 32, height = 32, RefillType = 2, State = true} },
        { name = "limboOff", data = {width = 32, height = 32, RefillType = 2, State = false} },
    },
    sprite = function(room, entity)
        local color = refill_colors[entity.RefillType]
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {color[1], color[2], color[3], 0.3}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, (entity.State and {1, 1, 1, 0.5}) or {0, 0, 0, 0.5})
        }
    end,
    fieldInformation = {
        RefillType = {
            type = "enum",
            options = { {"Midair", 0}, {"Overcharge", 1}, {"Limbo", 2} }
        },
    }
}
