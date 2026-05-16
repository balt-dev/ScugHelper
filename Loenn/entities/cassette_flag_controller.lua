local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local function spanValidator(str)
    for segment in string.gmatch(str, "[^;]+") do
        local seg = string.match(segment, "^%s*(.-)%s*$")
        if not string.match(seg, "^%S+%s+%d+%-%d+$") then
            return false
        end
    end
    return true
end

return {
    name = "ScugHelper/CassetteFlagController",
    depth = -20000,
    placements = {
        {
            name = "normal",
            data = { Spans = "FirstHalf 1-8;SecondHalf 9-16", Length = 16 }
        },
    },
    fieldInformation = { Spans = { fieldType = "list", elementSeparator = ";", minimumElements = 1, validator = spanValidator, elementDefault = "FirstHalf 1-8" }, Length = { fieldType = "integer" } },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Cassette Flag\nController",
                entity.x - 24, entity.y - 12, 48, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 24, entity.y - 12, 48, 24, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 24,
            entity.y - 12,
            48, 24
        )
    end
}
