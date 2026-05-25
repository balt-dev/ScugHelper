local utils = require("utils")

return {
    name = "ScugHelper/UnstuckTrigger",
    depth = 100,
    placements = {
        {
            name = "trigger",
            data = {width = 32, height = 32, AmountX = 8, AmountY = 8},
        },
    },
    fieldInformation = {
        AmountX = { fieldType = "integer" },
        AmountY = { fieldType = "integer" }
    },
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1}
}
