local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local kevin = {}

kevin.name = "ScugHelper/SeekerCrushBlock"
kevin.depth = 0
kevin.warnBelowSize = {24, 24}
kevin.fieldInformation = {
    FaceBackground = {fieldType = "color"}
}
kevin.placements = {
    name = "normal",
    data = {
        width = 24,
        height = 24,
        chillout = false,
        SpritePath = "objects/ScugHelper/playerCrushBlock",
        NoReturn = false,
        FaceBackground = "343e4e"
    }
}


local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

function kevin.sprite(room, entity)
    local frameTextures = {
        none = entity.SpritePath .. "/block00",
        horizontal = entity.SpritePath .. "/block01",
        vertical = entity.SpritePath .. "/block02",
        both = entity.SpritePath .. "/block03"
    }
    local smallFaceTexture = entity.SpritePath .. "/idle_face"
    local giantFaceTexture = entity.SpritePath .. "/giant_block00"
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local axes = "horizontal"
    local chillout = entity.chillout

    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = giant and giantFaceTexture or smallFaceTexture

    local frameTexture = frameTextures[axes] or frameTextures["both"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local color = scughelper.parseColor(entity.FaceBackground)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, color)
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

return kevin
