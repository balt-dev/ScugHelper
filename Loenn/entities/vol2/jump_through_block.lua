local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local fakeTilesHelper = require("helpers.fake_tiles")

local populate = scughelper.populateDefaults {
    blendin = true,
    width = 8,
    height = 8
}

local jumpThroughBlock = {}

jumpThroughBlock.name = "ScugHelper/JumpThroughBlock"
jumpThroughBlock.depth = function(room, entity)
    populate(entity)
    return -10000
end

function jumpThroughBlock.placements()
    return {
        {
            name = "up",
            data = populate {
                tiletype = fakeTilesHelper.getPlacementMaterial(),
                Direction = 0
            }
        },
        {
            name = "down",
            data = populate {
                tiletype = fakeTilesHelper.getPlacementMaterial(),
                Direction = 1
            }
        },
        {
            name = "left",
            data = populate {
                tiletype = fakeTilesHelper.getPlacementMaterial(),
                Direction = 2
            }
        },
        {
            name = "right",
            data = populate {
                tiletype = fakeTilesHelper.getPlacementMaterial(),
                Direction = 3
            }
        }
    }
end

jumpThroughBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin", "tilesFg")
jumpThroughBlock.fieldInformation = function()
    return {
        ["tiletype"] = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        Direction = {
            options = {Up = 0, Down = 1, Left = 2, Right = 3}
        },
    }
end

return jumpThroughBlock
