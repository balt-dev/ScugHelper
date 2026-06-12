local drawableSprite = require("structs.drawable_sprite")

local watchtower = {}

watchtower.name = "ScugHelper/SeekablePlaybackWatchtower"
watchtower.depth = -8500
watchtower.justification = {0.5, 1.0}
watchtower.nodeLineRenderType = "line"
watchtower.texture = "objects/lookout/lookout05"
watchtower.nodeLimits = {1, 1}
watchtower.placements = {
    name = "normal",
    data = {
        ScrollSpeed = 1,
        tutorial = "wavedash",
        KeepCameraInBounds = true
    }
}
watchtower.nodeSprite = function(room, entity, node, nodeIndex)
    local sprite = drawableSprite.fromTexture("characters/player_playback/launchRecover09", node)
    sprite:addPosition(0, -16)
    sprite:setColor {172 / 255, 50 / 255, 50 / 255}
    return sprite
end

return watchtower
