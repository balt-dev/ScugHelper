local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")

return {
    name = "ScugHelper/AngleBumper",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = {
                Angle = 0,
                Sprite = "angleBumper",
                DashCount = 1,
                LaunchSpeed = 280,
                RefillStamina = true,
                MiddleDecal = "objects/ScugHelper/angleBumper/middle",
            },
        }
    },
    sprite = function(room, entity)
        local base = drawableSprite.fromTexture("objects/ScugHelper/angleBumper/Idle21", entity)
        base.x = entity.x
        base.y = entity.y
        return {
            base,
            drawableFunc.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                    local targetX = entity.x + math.cos(math.pi * (entity.Angle / 180)) * (entity.LaunchSpeed / 16);
                    local targetY = entity.y - math.sin(math.pi * (entity.Angle / 180)) * (entity.LaunchSpeed / 16);
                    love.graphics.setColor({ 0.5, 0.8, 1, 0.8 })
                    love.graphics.line(entity.x, entity.y, targetX, targetY);
                end)
            end)
        }
    end,
    fieldInformation = { DashCount = {fieldType = "integer"}},
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 16,
            entity.y - 16,
            32, 32
        )
    end
}
