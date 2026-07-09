local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")

return {
    name = "ScugHelper/RemovePlayerTrigger",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32},
        },
    },
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1},
}
