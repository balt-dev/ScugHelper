local utils = require("utils")
local drawing = require("utils.drawing")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/SpeedcheckGate",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {
                Angle = 0,
                Size = 32,
                Threshold = 240,
                Axis = 0,
                Comparison = 0,
                Action = 0,
                FlagName = "",
                SetX = 0,
                SetY = 0,
                ChangeX = true,
                ChangeY = true,
            }
        },
    },
    sprite = function(room, entity)
        return scughelper.drawableGate(entity.x, entity.y, entity.Angle, entity.Size, {0, 1, 1})
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 8,
            entity.y - 8,
            16, 16
        )
    end,
    fieldInformation = {
        Axis = {
            type = "enum",
            options = { {"Horizontal", 0}, {"Vertical", 1}, {"Total", 2} }
        },
        Comparison = {
            type = "enum",
            options = { {"<", 0}, {"<=", 1}, {">", 2}, {">=", 3} }
        },
        Action = {
            type = "enum",
            options = { {"Kill", 0}, {"SetFlag", 1}, {"SetSpeed", 2} }
        }
    }
}
