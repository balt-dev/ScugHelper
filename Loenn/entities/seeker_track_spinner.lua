local drawableSpriteStruct = require("structs.drawable_sprite")

local rotateSpinner = {}

local nodeAlpha = 0.3

rotateSpinner.name = "ScugHelper/SeekerTrackSpinner"
rotateSpinner.nodeLimits = {1, 1}
rotateSpinner.nodeLineRenderType = "line"
rotateSpinner.depth = -12000
rotateSpinner.placements = {
    { name = "slow", data = { PauseTime = 0.3, MoveTime = 0.9 } },
    { name = "normal", data = { PauseTime = 0.2, MoveTime = 0.4 } },
    { name = "fast", data = { PauseTime = 0.6, MoveTime = 0.3 } },
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
