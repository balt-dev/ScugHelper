local utils = require("utils")

return {
    name = "ScugHelper/LevelEndTrigger",
    depth = 100,
    placements = {
        {
            name = "trigger",
            data = {width = 32, height = 32, ShowCompleteScreen = true, ShowSpotlight = true, ScreenWipe = true},
        },
    },
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1}
}
