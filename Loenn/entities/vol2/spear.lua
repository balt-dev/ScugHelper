local utils = require("utils")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

local populate = scughelper.populateDefaults { Sprite = "objects/ScugHelper/spear/metal" }

return {
    name = "ScugHelper/Spear",
    depth = -20,
    placements = {
        {
            name = "normal",
            data = {}
        }
    },
    texture = function(room, entity)
        populate(entity)
        return entity.Sprite
    end,
    selection = function(room, entity)
        return utils.rectangle(entity.x - 12, entity.y - 2, 24, 4)
    end,
}
