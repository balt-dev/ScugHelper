local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local goldenBlock = {}

goldenBlock.name = "ScugHelper/BrassBerryBlock"
goldenBlock.depth = -10000
goldenBlock.warnBelowSize = {16, 16}
goldenBlock.placements = {
    name = "brassBerryBlock",
    data = {
        width = 16,
        height = 16
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local blockTexture = "objects/ScugHelper/brassBerry/brassBerryBlock00"
local middleTexture = "objects/ScugHelper/brassBerry/idle00"

function goldenBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local middleSprite = drawableSprite.fromTexture(middleTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, middleSprite)

    return sprites
end

return goldenBlock
