local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/SessionExpressionListener",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Targets = "", Expression = "", Invert = false, width = 32, height = 24, Immediate = false}
        },
    },
    associatedMods = {"FrostHelper", "ScugHelper"},
    fieldInformation = { Targets = {fieldType = "list"}, Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Session Expression Listener\nWhen: %s%s\n%s"):format((entity.Invert and "!") or "", entity.Expression, entity.Targets),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "expressionAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
