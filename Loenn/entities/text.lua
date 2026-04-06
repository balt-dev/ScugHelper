local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")

local function getTextColor(entity)
    local success, r, g, b = utils.parseHexColor(entity.Infill)
    return success and {r, g, b} or {1, 1, 1}
end

return {
    name = "ScugHelper/Text",
    depth = 10,
    placements = {
        {
            name = "normal",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, RequiresUpdate = false, Value = "Hello, world!", Depth = 10}
        },
    },
    sprite = function(room, entity)
        return drawableText.fromText(entity.Value:gsub("\\\\", "\0"):gsub("\\{", "{"):gsub("\\}", "}"):gsub("\\n", "\n"):gsub("\0", "\\"), entity.x, entity.y, nil, nil, nil, 1, getTextColor(entity))
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
