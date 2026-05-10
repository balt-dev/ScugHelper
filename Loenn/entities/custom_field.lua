local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
return {
    name = "ScugHelper/NewCustomField",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32, Invisible = false, Names = "player", Invert = false, Color = "B3D9FF"}
        },
    },
    fieldInformation = { Names = { fieldType = "list", searchable = true }, Color = { fieldType = "color" }},
    sprite = function(room, entity)
        local r, g, b = table.unpack(scughelper.parseColor(entity.Color))
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {r, g, b, 0.3}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {1, 1, 1, 0.5})
        }
    end,
}
