local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRect = require("structs.drawable_rectangle")
return {
    name = "ScugHelper/RefillRectangle",
    depth = 5000,
    placements = {
        {
            name = "green",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "green", width = 32, height = 32, OutlineColor = "93bd40", InfillColor = "208020", InfillOpacity = 0.3 }
        },
        {
            name = "pink",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "pink", width = 32, height = 32, OutlineColor = "fb94ff", InfillColor = "e268d1", InfillOpacity = 0.3 }
        },
        {
            name = "blue",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "blue", width = 32, height = 32, OutlineColor = "5ed0f6", InfillColor = "30639d", InfillOpacity = 0.3 }
        },
        {
            name = "black",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "black", width = 32, height = 32, OutlineColor = "433655", InfillColor = "000000", InfillOpacity = 0.3 }
        },
        {
            name = "rose",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "rose", width = 32, height = 32, OutlineColor = "f65e89", InfillColor = "9d304f", InfillOpacity = 0.3 }
        },
        {
            name = "cyan",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "cyan", width = 32, height = 32, OutlineColor = "a5adff", InfillColor = "5369b3", InfillOpacity = 0.3 }
        },
        {
            name = "dark_green",
            data = { FallbackRefillOneUse = false, FallbackRefillType = "dark_green", width = 32, height = 32, OutlineColor = "7ac533", InfillColor = "435e28", InfillOpacity = 0.3 }
        },
    },
    sprite = function(room, entity)
        local infillColor = scughelper.parseColor(entity.InfillColor)
        infillColor[4] = entity.InfillOpacity
        return {
            drawableRect.fromRectangle("fill", entity.x + 2, entity.y + 2, entity.width - 4, entity.height - 4,
                infillColor),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height,
                scughelper.parseColor(entity.OutlineColor))
        }
    end
}
