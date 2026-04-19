local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
return {
    name = "ScugHelper/CustomField",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32, Invisible = false, Types = "Celeste.Player", Invert = false, Color = "B3D9FF"}
        },
    },
    fieldInformation = { Types = { fieldType = "list" }, Color = { fieldType = "color" }},
    sprite = function(room, entity)
        local r, g, b = table.unpack(scughelper.parseColor(entity.Color))
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {r, g, b, 0.3}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {1, 1, 1, 0.5})
        }
    end,
}
