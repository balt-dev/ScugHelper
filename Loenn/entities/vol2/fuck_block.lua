local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local fakeTilesHelper = require("helpers.fake_tiles")

local populate = scughelper.populateDefaults {
    blendin = true,
    width = 8,
    height = 8,
    State = 0,
    IgnoreStateLock = false
}

local fuckBlock = {}

fuckBlock.name = "ScugHelper/FuckBlock"
fuckBlock.depth = function(room, entity)
    populate(entity)
    return -10000
end

function fuckBlock.placements()
    return {
        {
            name = "normal",
            data = populate {
                tiletype = fakeTilesHelper.getPlacementMaterial(),
            }
        },
        {
            name = "nostatelock",
            data = populate {
                IgnoreStateLock = true,
                tiletype = fakeTilesHelper.getPlacementMaterial(),
            }
        }
    }
end

fuckBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin", "tilesFg")
fuckBlock.fieldInformation = function()
    return {
        ["tiletype"] = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        State = {fieldType = "integer", options = scughelper.playerStates, editable = true, searchable = true }
    }
end

return fuckBlock
