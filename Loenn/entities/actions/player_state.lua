local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/PlayerStateAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, width = 32, height = 24, State = 0}
        },
    },
    fieldInformation = {
        Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" },
        State = {fieldType = "integer", options = scughelper.playerStates, editable = true, searchable = true }
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Set Player State\n%s (%.3fs)\n%s"):format(entity.Groups, entity.Delay, scughelper.playerStateNames[entity.State] or tostring(entity.State)),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "playerAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
