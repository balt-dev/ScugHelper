local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/ForwardAction",
    depth = -1e10,
    placements = {
        {
            name = "normal",
            data = {Groups = "", Delay = 0, Targets = "", width = 32, height = 24}
        },
    },
    fieldInformation = { Targets = {fieldType = "list"}, Groups = {fieldType = "list"}},
    sprite = function(room, entity)
        return drawableText.fromText(
            ("Action: Forward\n%s (%.3fs)\nTargets: %s"):format(entity.Groups, entity.Delay, entity.Targets),
            entity.x, entity.y, entity.width, entity.height, nil, 0.25
        )
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
