local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local conveyor = {}

conveyor.name = "ScugHelper/Conveyor"
conveyor.depth = 1999
conveyor.canResize = { true, false }
conveyor.placements = {
    {
        name = "normal",
        placementType = "rectangle",
        data = {
            width = 32,
            SpritePath = "objects/ScugHelper/conveyor",
            TargetSpeed = 60,
            Flip = false
        }
    }
}

function conveyor.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local width = entity.width or 8
    local tileWidth = math.floor(width / 4)

    local leftTex, midTex, rightTex = entity.SpritePath .. "/left00", entity.SpritePath .. "/middle00",
        entity.SpritePath .. "/right00"

    for i = 2, tileWidth - 1 do
        local middleSprite = drawableSprite.fromTexture(midTex, entity)
        middleSprite:addPosition((i - 1) * 4, 4)
        middleSprite:setScale(1, (entity.Flip and -1) or 1)
        middleSprite:setJustification(0.0, 0.0)
        table.insert(sprites, middleSprite)
    end

    local leftSprite = drawableSprite.fromTexture(leftTex, entity)
    local rightSprite = drawableSprite.fromTexture(rightTex, entity)

    leftSprite:addPosition(0, 4)
    leftSprite:setScale(1, (entity.Flip and -1) or 1)
    leftSprite:setJustification(0.0, 0.0)

    rightSprite:addPosition((tileWidth - 1) * 4, 4)
    rightSprite:setScale(1, (entity.Flip and -1) or 1)
    rightSprite:setJustification(0.0, 0.0)

    table.insert(sprites, leftSprite)
    table.insert(sprites, rightSprite)

    return sprites
end

function conveyor.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 8, 8)
end

return conveyor
