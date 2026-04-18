local drawableRect = require("structs.drawable_rectangle")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/SetInventoryAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, width = 32, height = 24, Dashes = 1, DreamDash = true, Backpack = true, NoRefills = false}
        },
    },
    fieldInformation = { Groups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""}},
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: SetInventory\n%s (%.3fs)\nDashes: %d\nDream Blocks: %s\nBackpack: %s\nNo Refills: %s"):format(
                    entity.Groups, entity.Delay,
                    entity.Dashes,
                    tostring(entity.DreamDash),
                    tostring(entity.Backpack),
                    tostring(entity.NoRefills)
                ),
                entity.x, entity.y, entity.width, entity.height, nil, 0.25
            ),
            drawableRect.fromRectangle("line", entity.x - 1, entity.y - 1, entity.width + 2, entity.height + 2, scughelper.colors "playerAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
