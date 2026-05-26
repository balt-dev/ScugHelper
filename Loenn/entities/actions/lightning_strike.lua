local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/LightningStrikeAction",
    depth = -1e10,
    nodeLineRenderType = "line",
    nodeLimits = { 1, 1 },
    placements = {
        {
            name = "normal",
            data = { Groups = "", Immediate = false, Delay = 0, width = 32, height = 24, BoltHeight = 128 }
        },
    },
    fieldInformation = { Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" } },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Lightning Strike\n%s (%.3fs)"):format(entity.Groups, entity.Delay),
                entity.x + 2, entity.y + 2, nil, nil, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "playerAction")
        }
    end,
    nodeRectangle = function(room, entity, node, nodeIndex, viewport)
        return utils.rectangle((node.x or 0) + entity.width / 2 - 2, (node.y or 0) + entity.height / 2 - 2, 4, 4)
    end,
    nodeFillColor = { 0, 0, 0, 0 },
    nodeBorderColor = { 1, 1, 1, 1 }
}
