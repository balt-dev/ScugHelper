local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local starjumpRailing = {}

starjumpRailing.name = "ScugHelper/StarjumpRailing"
starjumpRailing.depth = function(room, entity) return entity.Depth end
starjumpRailing.warnBelowSize = { 8, 8 }
starjumpRailing.placements = {
    {
        name = "normal",
        data = {
            width = 32,
            Depth = -10,
            Flip = false
        }
    },
    {
        name = "flipped",
        data = {
            width = 32,
            Depth = -10,
            Flip = true
        }
    },
    {
        name = "background",
        data = {
            width = 32,
            Depth = 10,
            Flip = false
        }
    },
    {
        name = "flipped_background",
        data = {
            width = 32,
            Depth = 10,
            Flip = true
        }
    }
}

local leftRailings = {
    "objects/starjumpBlock/leftrailing00",
    "objects/starjumpBlock/leftrailing01",
    "objects/starjumpBlock/leftrailing02",
    "objects/starjumpBlock/leftrailing03",
    "objects/starjumpBlock/leftrailing04",
    "objects/starjumpBlock/leftrailing05",
    "objects/starjumpBlock/leftrailing06"
}
local rightRailings = {
    "objects/starjumpBlock/rightrailing00",
    "objects/starjumpBlock/rightrailing01",
    "objects/starjumpBlock/rightrailing02",
    "objects/starjumpBlock/rightrailing03",
    "objects/starjumpBlock/rightrailing04",
    "objects/starjumpBlock/rightrailing05",
    "objects/starjumpBlock/rightrailing06"
}
local railings = {
    "objects/starjumpBlock/railing00",
    "objects/starjumpBlock/railing01",
    "objects/starjumpBlock/railing02",
    "objects/starjumpBlock/railing03",
    "objects/starjumpBlock/railing04",
    "objects/starjumpBlock/railing05",
    "objects/starjumpBlock/railing06"
}

local function getRailingSprite(entity, textures, offsetX, offsetY)
    local texture = textures[utils.mod1(math.floor((entity.x + offsetX) / 8), #textures)]

    if texture then
        local sprite = drawableSprite.fromTexture(texture, entity)

        sprite:addPosition(offsetX, offsetY)
        sprite:setScale(1, (entity.Flip and -1) or 1)
        sprite:setJustification(0.0, 0.0)

        return sprite
    end
end

local function addRailingSprite(sprites, entity, textures, offsetX, offsetY)
    local sprite = getRailingSprite(entity, textures, offsetX, offsetY)

    if sprite then
        table.insert(sprites, sprite)
    end
end

function starjumpRailing.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 16
    if width <= 8 then
        addRailingSprite(sprites, entity, railings, 0, (entity.Flip and 0) or -8)
        return sprites
    end
    addRailingSprite(sprites, entity, leftRailings, 0, (entity.Flip and 0) or -8)
    for w = 8, width - 16, 8 do
        addRailingSprite(sprites, entity, railings, w, (entity.Flip and 0) or -8)
    end
    addRailingSprite(sprites, entity, rightRailings, width - 8, (entity.Flip and 0) or -8)
    return sprites
end

return starjumpRailing
