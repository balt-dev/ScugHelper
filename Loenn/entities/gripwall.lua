local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local gripwall = {}

gripwall.name = "ScugHelper/Gripwall"
gripwall.depth = 1999
gripwall.canResize = {false, true}
gripwall.placements = {
    {
        name = "gripwall_right",
        placementType = "rectangle",
        data = {
            height = 8,
            left = true,
            refillDash = true,
            refillStamina = true,
        }
    },
    {
        name = "gripwall_left",
        placementType = "rectangle",
        data = {
            height = 8,
            left = false,
            refillDash = true,
            refillStamina = true,
        }
    },
}

local topTexture = "objects/gripwall/bothTop0"
local middleTexture = "objects/gripwall/bothMid0"
local bottomTexture = "objects/gripwall/bothBottom0"

function gripwall.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

    for i = 2, tileHeight - 1 do
        local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition(offsetX, (i - 1) * 8)
        middleSprite:setScale(scaleX, 1)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local topSprite = drawableSprite.fromTexture(topTexture, entity)
    local bottomSprite = drawableSprite.fromTexture(bottomTexture, entity)

    topSprite:addPosition(offsetX, 0)
    topSprite:setScale(scaleX, 1)
    topSprite:setJustification(0.0, 0.0)

    bottomSprite:addPosition(offsetX, (tileHeight - 1) * 8)
    bottomSprite:setScale(scaleX, 1)
    bottomSprite:setJustification(0.0, 0.0)

    table.insert(sprites, topSprite)
    table.insert(sprites, bottomSprite)

    return sprites
end

function gripwall.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height or 8)
end

function gripwall.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.left = not entity.left
        entity.x = entity.x + (entity.left and 8 or -8)
    end

    return horizontal
end

return gripwall