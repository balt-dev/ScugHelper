local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/Text",
    depth = function(room, entity) return entity.Depth end,
    placements = {
        {
            name = "normal",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, RequiresUpdate = false, Value = "Hello, world!", Depth = 10}
        },
    },
    sprite = function(room, entity)
        return drawableText.fromText(entity.Value:gsub("\\\\", "\0"):gsub("\\{", "{"):gsub("\\}", "}"):gsub("\\n", "\n"):gsub("\0", "\\"), entity.x, entity.y, nil, nil, nil, 1, scughelper.parseColor(entity.Infill))
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, 8, 8)
    end,
    fieldInformation = {
        Depth = { fieldType = "integer" },
        Infill = { fieldType = "color" },
        Outline = { fieldType = "color" }
    }
}
