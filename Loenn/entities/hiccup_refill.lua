local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/HiccupRefill",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = {
                oneUse = false
            }
        }
    },
    texture = "objects/hiccupRefill/idle00",
    selection = function(room, entity)
        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    end,
}
