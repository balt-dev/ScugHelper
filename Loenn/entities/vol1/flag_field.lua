local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
return {
    name = "ScugHelper/FlagField",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { width = 32, height = 32, Invisible = false, Flag = "", FlagState = true, Color = "B3D9FF" }
        },
    },
    fieldInformation = { Flag = scughelper.builtinFlags, Color = { fieldType = "color" } },
    sprite = function(room, entity)
        local r, g, b = table.unpack(scughelper.parseColor(entity.Color))
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, { r, g, b, 0.3 }),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, { 1, 1, 1, 0.5 }),
            drawableText.fromText(
                "Flag Barrier\n" .. entity.Flag,
                entity.x + 2, entity.y + 2, entity.width - 4, entity.height - 4, nil, 0.5
            )
        }
    end
}
