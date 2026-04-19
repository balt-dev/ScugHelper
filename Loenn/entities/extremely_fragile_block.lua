local fakeTilesHelper = require("helpers.fake_tiles")

local extremelyFragileBlock = {}

extremelyFragileBlock.name = "ScugHelper/ExtremelyFragileBlock"
extremelyFragileBlock.depth = 0

function extremelyFragileBlock.placements()
    return {
        name = "normal",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            blendin = true,
            permanent = true,
            width = 8,
            height = 8
        }
    }
end

extremelyFragileBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
extremelyFragileBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return extremelyFragileBlock