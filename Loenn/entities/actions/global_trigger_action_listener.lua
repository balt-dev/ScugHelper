local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/GlobalTriggerActionListener",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = ""}
        },
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
            ("Global Trigger Action Listener\nTriggers on: %s"):format(entity.Groups),
            entity.x, entity.y + 3, nil, nil, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x - 2, entity.y - 2, 4, 4, {1, 1, 1}, {0, 0, 0, 0})
        }
    end,
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    rectangle = function(room, entity)
        return utils.rectangle(entity.x - 2, entity.y - 2, 4, 4)
    end
}
