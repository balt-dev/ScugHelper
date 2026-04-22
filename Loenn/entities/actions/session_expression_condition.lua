local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/SessionExpressionConditionAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Targets = "", Delay = 0, Invert = false, Expression = "", width = 32, height = 24}
        },
    },
    associatedMods = {"FrostHelper", "ScugHelper"},
    fieldInformation = { Targets = {fieldType = "list"}, Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Session Expression Condition\n%s (%.3fs)\nIf: %s%s\n%s"):format(entity.Groups, entity.Delay, (entity.Invert and "!") or "", entity.Expression, entity.Targets),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "expressionAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
