local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")

return {
    name = "ScugHelper/ActionGate",
    depth = 100,
    nodeLineRenderType = "line",
    nodeLimits = {1, 1},
    placements = {
        {
            name = "normal",
            data = {Angle = 0, Size = 32, Targets = ""},
        },
    },
    fieldInformation = { Targets = {fieldType = "list"}},
    sprite = function(room, entity)
        return scughelper.drawableGate(entity.x, entity.y, entity.Angle, entity.Size, {0, 1, 1})
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 8,
            entity.y - 8,
            16, 16
        )
    end
}
