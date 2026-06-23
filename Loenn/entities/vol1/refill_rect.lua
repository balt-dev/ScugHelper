local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRect = require("structs.drawable_rectangle")
local populate = scughelper.populateDefaults {
    FallbackRefillOneUse = false,
    width = 32, height = 32,
    InfillOpacity = 0.3,
    RespawnTime = 2.5
}
return {
    name = "ScugHelper/RefillRectangle",
    depth = 5000,
    placements = {
        {
            name = "green",
            data = populate { FallbackRefillType = "green", OutlineColor = "93bd40", InfillColor = "208020" }
        },
        {
            name = "pink",
            data = populate { FallbackRefillType = "pink", OutlineColor = "fb94ff", InfillColor = "e268d1" }
        },
        {
            name = "blue",
            data = populate { FallbackRefillType = "blue", OutlineColor = "5ed0f6", InfillColor = "30639d" }
        },
        {
            name = "black",
            data = populate { FallbackRefillType = "black", OutlineColor = "433655", InfillColor = "000000" }
        },
        {
            name = "rose",
            data = populate { FallbackRefillType = "rose", OutlineColor = "f65e89", InfillColor = "9d304f" }
        },
        {
            name = "cyan",
            data = populate { FallbackRefillType = "cyan", OutlineColor = "a5adff", InfillColor = "5369b3" }
        },
        {
            name = "gold",
            data = populate { FallbackRefillType = "gold", OutlineColor = "d3c487", InfillColor = "a17b4a" }
        },
        {
            name = "dark_green",
            data = populate { FallbackRefillType = "dark_green", OutlineColor = "7ac533", InfillColor = "435e28" }
        },
    },
    fieldInformation = {
        OutlineColor = { fieldType = "color" }, InfillColor = { fieldType = "color" },
    },
    sprite = function(room, entity)
        populate(entity)
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
