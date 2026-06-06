local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/FlagListener",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 24, Flag = "ScugHelper.PlayerOnGround", State = true, Targets = ""}
        },
    },
    fieldInformation = { Targets = {fieldType = "list"}, Flag = scughelper.builtinFlags },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Flag Listener\n%s\nOn: %s"):format(entity.Targets, ((entity.State == false and "!") or "") .. entity.Flag),
                entity.x + 2, entity.y + 2, nil, nil, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "metaAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
