local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/SessionExpressionAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, Expression = "", Target = "", width = 32, height = 24}
        },
    },
    associatedMods = {"FrostHelper", "ScugHelper"},
    fieldInformation = {
        Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" },
        Target = {
            options = {
                { "$player.x", "$player.x" },
                { "$player.y", "$player.y" },
                { "$subpixel.x", "$subpixel.x" },
                { "$subpixel.y", "$subpixel.y" },
                { "$speed.x", "$speed.x" },
                { "$speed.y", "$speed.y" },
                { "$stamina", "$stamina" },
                { "$dashes", "$dashes" },
            },
            searchable = true,
            editable = true
        }
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Session Expression\n%s (%.3fs)\n%s <- %s"):format(entity.Groups, entity.Delay, entity.Target, entity.Expression),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "expressionAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
