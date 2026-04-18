local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/PauseAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, width = 32, height = 24}
        },
    },
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Pause Game\n%s (%.3fs)"):format(entity.Groups, entity.Delay),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x - 1, entity.y - 1, entity.width + 2, entity.height + 2, scughelper.colors "playerAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
