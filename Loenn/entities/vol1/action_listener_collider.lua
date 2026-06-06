local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawing = require("utils.drawing")

return {
    name = "ScugHelper/ActionListenerCollider",
    depth = -1000000,
    nodeLineRenderType = "line",
    nodeLimits = {1, 1},
    placements = {
        {
            name = "normal",
            data = {Groups = ""},
        },
    },
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Action Listener",
                entity.x - 20, entity.y - 12, 40, 16, nil, 1
            ),
            drawableText.fromText(
                entity.Groups,
                entity.x - 20, entity.y + 4, 40, 8, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x - 20, entity.y - 12, 40, 24, scughelper.colors "metaAction")
        }
    end,
    selection = function(room, entity)
        local nodes = entity.nodes or {}
        local nodeX, nodeY = nodes[1].x or entity.x, nodes[1].y or entity.y
        return utils.rectangle(
            entity.x - 20,
            entity.y - 12,
            40, 24
        ), { utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4) }
    end,
    nodeRectangle = function (room, entity, node, nodeIndex, viewport)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        return utils.rectangle((nodeX or 0) - 2, (nodeY or 0) - 2, 4, 4)
    end,
    nodeFillColor = {0, 0, 0, 0},
    nodeBorderColor = {1, 1, 1, 1}
}
