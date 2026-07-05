local utils = require("utils")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

local populate = scughelper.populateDefaults { Texture = "objects/ScugHelper/anvil", KillOnDestroy = false, NoLeaveBehind = false, CrystalSounds = false }

return {
    name = "ScugHelper/Anvil",
    depth = -20,
    placements = {
        {
            name = "normal",
            data = {}
        },
        {
            name = "theo",
            data = {
                Texture = "objects/ScugHelper/theoAnvil",
                KillOnDestroy = true,
                NoLeaveBehind = true,
                CrystalSounds = true
            }
        }
    },
    texture = function(room, entity)
        populate(entity)
        return entity.Texture
    end,
    selection = function(room, entity)
        return utils.rectangle(entity.x - 4, entity.y - 2, 8, 6)
    end,
}
