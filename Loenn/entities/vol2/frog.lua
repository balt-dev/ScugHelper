local drawableSpriteStruct = require("structs.drawable_sprite")
local utils = require("utils")

local frog = {}

frog.name = "ScugHelper/Frog"
frog.depth = -9999
frog.placements = {
    name = "normal", data = { Color = "20a030" }
}

local texture = "objects/ScugHelper/frog"

function frog.sprite(room, entity)
    utils.setSimpleCoordinateSeed(entity.x, entity.y)
    local spr = drawableSpriteStruct.fromTexture(texture, entity)

    spr:setJustification(0.5, 1.0)
    spr:setColor(entity.Color)

    return spr
end

return frog
