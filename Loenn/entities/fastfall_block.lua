local fakeTilesHelper = require("helpers.fake_tiles")

local fastfallBlock = {}

fastfallBlock.name = "ScugHelper/FastfallBlock"
fastfallBlock.depth = -10000

function fastfallBlock.placements()
    return {
        name = "normal",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            blendin = true,
            permanent = true,
            width = 8,
            height = 8,
            SpeedMinimum = 240
        }
    }
end

fastfallBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin", "tilesFg")
fastfallBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return fastfallBlock