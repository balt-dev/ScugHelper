local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/WaterColorController",
    depth = -1e20,
    placements = {
        {
            name = "normal",
            data = {
                Fill = "87CEFA",
                FillOpacity = 0.3,
                Surface = "87CEFA",
                SurfaceOpacity = 0.8,
                RayTop = "87CEFA",
                RayTopOpacity = 0.6
            }
        },
    },
    sprite = function(room, entity)
        local fill = scughelper.parseColor(entity.Fill)
        fill[4] = entity.FillOpacity
        local surface = scughelper.parseColor(entity.Surface)
        surface[4] = entity.SurfaceOpacity
        return {
            drawableText.fromText(
                "Water Color\nController",
                entity.x - 24, entity.y - 12, 48, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 24, entity.y - 12, 58, 24, { 1, 1, 1, 1 }),
            drawableRect.fromRectangle("fill", entity.x + 25, entity.y - 9, 6, 6, fill),
            drawableRect.fromRectangle("line", entity.x + 24, entity.y - 10, 8, 8, surface)
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 24,
            entity.y - 12,
            58, 24
        )
    end,
    fieldInformation = {
        Fill = { fieldType = "color" },
        Surface = { fieldType = "color" },
        RayTop = { fieldType = "color" },
    }
}
