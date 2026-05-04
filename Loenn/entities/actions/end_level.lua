local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/LevelEndAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Immediate = false, Delay = 0, width = 32, height = 24, ShowCompleteScreen = true, ShowSpotlight = true, ScreenWipe = true}
        },
    },
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: End Level\n%s (%.3fs)"):format(entity.Groups, entity.Delay),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "playerAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
