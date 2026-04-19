local drawableFunc = require("structs.drawable_function")
local utils = require("utils")
local drawing = require("utils.drawing")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/Outline",
    depth = function(room, entity) return entity.Depth end,
        fieldInformation = {
            Color = { fieldType = "color" },
        },
    placements = {
        {
            name = "normal",
            data = {
                width = 8,
                height = 8,
                Depth = 10,
                Color = "FFFFFF",
                InnerOpacity = 0.25,
                LineSize = 2,
                SpaceSize = 1,
                CornerSize = 2,
                InnerMargin = 4,
                CornerSpace = 1,
            }
        },
    },

    sprite = function(room, entity)
        entity.Depth = entity.Depth or 10
        entity.Color = entity.Color or "FFFFFF"
        entity.InnerOpacity = entity.InnerOpacity or 0.25
        entity.LineSize = entity.LineSize or 2
        entity.SpaceSize = entity.SpaceSize or 1
        entity.CornerSize = entity.CornerSize or 2
        entity.InnerMargin = entity.InnerMargin or 4
        entity.CornerSpace = entity.CornerSpace or 1
        if entity.SpaceSize + entity.LineSize <= 0 then
            entity.LineSize = 0
            entity.SpaceSize = 1
        end
        return drawableFunc.fromFunction(function()
            drawing.callKeepOriginalColor(function()
                local left   = entity.x
                local top    = entity.y
                local right  = entity.x + entity.width
                local bottom = entity.y + entity.height
                local color = scughelper.parseColor(entity.Color)
                love.graphics.setColor(color[1], color[2], color[3], entity.InnerOpacity)
                love.graphics.rectangle("fill",
                    left + entity.InnerMargin,
                    top + entity.InnerMargin,
                    entity.width - entity.InnerMargin * 2,
                    entity.height - entity.InnerMargin * 2
                )
                love.graphics.setColor(color[1], color[2], color[3], 1)
                for x = left + entity.CornerSize + entity.CornerSpace, right - entity.CornerSize - entity.CornerSpace - entity.LineSize, entity.LineSize + entity.SpaceSize do
                    love.graphics.line(x, top + 0.5, x + entity.LineSize, top + 0.5)
                    love.graphics.line(x, bottom - 0.5, x + entity.LineSize, bottom - 0.5)
                end
                for y = top + entity.CornerSize + entity.CornerSpace, bottom - entity.CornerSize - entity.CornerSpace - entity.LineSize, entity.LineSize + entity.SpaceSize do
                    love.graphics.line(left + 0.5, y, left + 0.5, y + entity.LineSize)
                    love.graphics.line(right - 0.5, y, right - 0.5, y + entity.LineSize)
                end
                love.graphics.rectangle("fill", left, top, entity.CornerSize, entity.CornerSize)
                love.graphics.rectangle("fill", right - entity.CornerSize, top, entity.CornerSize, entity.CornerSize)
                love.graphics.rectangle("fill", left, bottom - entity.CornerSize, entity.CornerSize, entity.CornerSize)
                love.graphics.rectangle("fill", right - entity.CornerSize, bottom - entity.CornerSize, entity.CornerSize, entity.CornerSize)
            end)
        end)
    end,
}
