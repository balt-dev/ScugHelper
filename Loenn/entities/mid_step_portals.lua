local utils = require("utils")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/MidStepPortals",
    depth = 100,
    nodeLimits = { 1, 1 },
    placements = {
        {
            name = "normal",
            data = { height = 64, Silent = false, Invisible = false }
        },
    },
    sprite = function(room, entity)
        return drawableRect.fromRectangle("fill", entity.x, entity.y, 1, entity.height, { 1, 0, 0, 1 })
    end,
    nodeSprite = function(room, entity)
        entity.nodes = entity.nodes or { { x = entity.x, y = entity.y } }
        return drawableRect.fromRectangle("fill", entity.nodes[1].x, entity.nodes[1].y, 1, entity.height, { 1, 1, 0, 1 })
    end,
    selection = function(room, entity)
        entity.nodes = entity.nodes or { { x = entity.x, y = entity.y } }
        return utils.rectangle(
            entity.x,
            entity.y,
            1, entity.height
        ), {
            utils.rectangle(
                entity.nodes[1].x,
                entity.nodes[1].y,
                1, entity.height
            )
        }
    end
}
