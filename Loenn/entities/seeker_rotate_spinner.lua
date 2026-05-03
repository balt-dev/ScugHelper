local drawableSpriteStruct = require("structs.drawable_sprite")

local rotateSpinner = {}

local nodeAlpha = 0.3

rotateSpinner.name = "ScugHelper/SeekerRotateSpinner"
rotateSpinner.nodeLimits = {1, 1}
rotateSpinner.nodeLineRenderType = "circle"
rotateSpinner.depth = -12000
rotateSpinner.placements = {
    { name = "clockwise", data = { Period = 1.8 } },
    { name = "counter_clockwise", data = { Period = -1.8 } }
}

local function getSprite(room, entity, alpha)
    local spr = drawableSpriteStruct.fromTexture("objects/seekerSpinner/fg00", entity)
    spr:setAlpha(alpha)
    return spr
end

function rotateSpinner.sprite(room, entity)
    return getSprite(room, entity)
end

function rotateSpinner.nodeSprite(room, entity, node)
    local entityCopy = table.shallowcopy(entity)

    entityCopy.x = node.x
    entityCopy.y = node.y

    return getSprite(room, entityCopy, nodeAlpha)
end

return rotateSpinner
