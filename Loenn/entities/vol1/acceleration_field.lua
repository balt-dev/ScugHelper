local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/NewAccelerationField",
    depth = -10000,
    placements = {
        {
            name = "normal",
            data = {
                width = 32,
                height = 32,
                Everywhere = false,
                FieldColor = "ADD8E6",
                FieldOpacity = 0.1,
                OutlineColor = "F0F8FF",
                OutlineOpacity = 0.2,
                TargetX = 240,
                TargetY = 240,
                AccelX = 900,
                AccelY = 900,
                BehaviorX = 2,
                BehaviorY = 2,
                DrawParticles = true
            }
        },
    },
    sprite = function(room, entity)
        local fieldColor = scughelper.parseColor(entity.FieldColor)
        local outlineColor = scughelper.parseColor(entity.OutlineColor)
        return {
            drawableRect.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {fieldColor[1], fieldColor[2], fieldColor[3], entity.FieldOpacity}),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {outlineColor[1], outlineColor[2], outlineColor[3], entity.OutlineOpacity})
        }
    end,
    fieldInformation = {
        BehaviorX = {
            fieldType = "integer",
            options = {
                {"Always", 0},
                {"Never", 1},
                {"LessThanVelocity", 2},
                {"GreaterThanVelocity", 3},
                {"LessThanMagnitude", 4},
                {"GreaterThanMagnitude", 5},
            },
        },
        BehaviorY = {
            fieldType = "integer",
            options = {
                {"Always", 0},
                {"Never", 1},
                {"LessThanVelocity", 2},
                {"GreaterThanVelocity", 3},
                {"LessThanMagnitude", 4},
                {"GreaterThanMagnitude", 5},
            },
        },
        FieldColor = { fieldType = "color" },
        OutlineColor = { fieldType = "color" }
    }
}
