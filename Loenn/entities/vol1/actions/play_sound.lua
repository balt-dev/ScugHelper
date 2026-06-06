local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/PlaySoundAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, width = 32, height = 24, Path = "", Immediate = false}
        },
    },
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: PlaySound\n%s (%.3fs)\n%s"):format(entity.Groups, entity.Delay, entity.Path),
                entity.x + 2, entity.y + 2, nil, nil, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "playerAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
