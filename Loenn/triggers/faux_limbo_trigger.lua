local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")

return {
    name = "ScugHelper/LimboGlitchStateTrigger",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 32, LockStateMachine = true, DetachSprite = true},
        },
    },
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1},
}
