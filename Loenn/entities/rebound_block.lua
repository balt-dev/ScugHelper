local drawableNinePatch = require("structs.drawable_nine_patch")

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local zeroBlock = "objects/ScugHelper/reboundBlock/zeroBlock"
local oneBlock = "objects/ScugHelper/reboundBlock/oneBlock"
local twoBlock = "objects/ScugHelper/reboundBlock/twoBlock"

local zeroSlot = "objects/ScugHelper/reboundBlock/zeroSlot"
local oneSlot = "objects/ScugHelper/reboundBlock/oneSlot"
local twoSlot = "objects/ScugHelper/reboundBlock/twoSlot"

return {
    name = "ScugHelper/ReboundBlock",
    depth = -10,
    placements = {
        {
            name = "reboundBlockGrey",
            data = { width = 32, height = 32, kind = 0 }
        },
        {
            name = "reboundBlockGreen",
            data = { width = 32, height = 32, kind = 1 }
        },
        {
            name = "reboundBlockGreen",
            data = { width = 32, height = 32, kind = 2 }
        },
    },
    sprite = function(room, entity)
        local x, y = entity.x or 0, entity.y or 0
        local width, height = entity.width or 24, entity.height or 24

        local blockTexture = ({ zeroBlock, oneBlock, twoBlock })[entity.kind + 1]
        local slotTexture = ({ zeroSlot, oneSlot, twoSlot })[entity.kind + 1]

        local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
        local sprites = ninePatch:getDrawableSprite()

        return sprites
    end,
    fieldInformation = {
        kind = {
            type = "integer",
            options = { { "Grey", 0 }, { "Green", 1 }, { "Pink", 2 } }
        }
    }
}
