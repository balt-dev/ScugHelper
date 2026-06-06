local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/GlobalEntityActionListener",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", width = 32, height = 32, Immediate = false}
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
            ("Global Entity Action Listener\nTriggers on: %s"):format(entity.Groups),
            entity.x, entity.y - 4, nil, nil, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {1, 1, 1}, {0, 0, 0, 0})
        }
    end,
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
