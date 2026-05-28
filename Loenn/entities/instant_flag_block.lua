local fakeTilesHelper = require("helpers.fake_tiles")

local instantFlagBlock = {}

instantFlagBlock.name = "ScugHelper/InstantFlagBlock"
instantFlagBlock.depth = -10000

function instantFlagBlock.placements()
    return {
        name = "normal",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            blendin = true,
            width = 8,
            height = 8,
            Flag = "",
            FlagState = false
        }
    }
end

instantFlagBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin", "tilesFg", {1.0, 1.0, 1.0, 0.7})
instantFlagBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return instantFlagBlock