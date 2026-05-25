local drawableSprite = require("structs.drawable_sprite")
local drawableRect = require("structs.drawable_rectangle")

local watchtower = {}

watchtower.name = "ScugHelper/ElsewhereWatchtower"
watchtower.depth = -8500
watchtower.justification = {0.5, 1.0}
watchtower.nodeLineRenderType = "line"
watchtower.texture = "objects/lookout/lookout05"
watchtower.nodeLimits = {1, -1}
watchtower.placements = {
    name = "watchtower",
    alternativeName = {"lookout", "binoculars"},
    data = {
        summit = false,
        onlyY = false
    }
}
watchtower.nodeSprite = function(room, entity, node, nodeIndex)
    if nodeIndex > 1 then return drawableSprite.fromTexture("objects/lookout/lookout05", node) end
    local camWidth = 40 * 8
    local camHeight = (23 * 8) - 4
    return drawableRect.fromRectangle("line", node.x, node.y, camWidth, camHeight, {1, 1, 1, 0.6})
end

return watchtower
