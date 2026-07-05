local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")

local populate = scughelper.populateDefaults {
    Color = "FFFFFF",
    Color2 = "FFFFFF",
    ColorFadeSlider = "",
    Flag = "",
    FlagState = true,
    Width = 2,
    NodeTexture = "objects/ScugHelper/polyline/defaultNode",
    TintNodeTexture = true,
    Depth = 5000
}

return {
    name = "ScugHelper/Polyline",
    depth = function(room, entity) return entity.Depth end,
    nodeLineRenderType = "none",
    nodeLimits = { 1, math.huge },
    placements = {
        {
            name = "normal",
            data = populate {},
        },
    },
    sprite = function(room, entity)
        local polylineTable = { entity.x, entity.y }
        for i = 1, #entity.nodes do
            polylineTable[#polylineTable + 1] = entity.nodes[i].x
            polylineTable[#polylineTable + 1] = entity.nodes[i].y
        end
        local sprites = {
            drawableFunc.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                    love.graphics.setColor(scughelper.parseColor(entity.Color))
                    love.graphics.setLineWidth(entity.Width)
                    love.graphics.setLineStyle "rough"
                    for i = 1, #polylineTable - 2, 2 do
                        love.graphics.line(polylineTable[i], polylineTable[i+1], polylineTable[i+2], polylineTable[i+3])
                    end
                    love.graphics.setColor({0, 0, 0, 0})
                    love.graphics.setLineWidth(1)
                end)
            end),
        }
        for i = 1, #polylineTable, 2 do
            local spr = drawableSprite.fromTexture(entity.NodeTexture, {x = polylineTable[i], y = polylineTable[i + 1]})
            if entity.TintNodeTexture then
                spr:setColor(scughelper.parseColor(entity.Color))
            end
            sprites[#sprites + 1] = spr
        end
        return sprites
    end,
    nodeSprite = function() return {} end,
    selection = function(room, entity)
        entity.nodes = entity.nodes or {}
        local nodeSelection = {}
        for i, node in pairs(entity.nodes) do
            nodeSelection[#nodeSelection+1] = utils.rectangle(node.x - 2, node.y - 2, 4, 4)
        end
        return utils.rectangle(entity.x - 2, entity.y - 2, 4, 4), nodeSelection
    end,
    fieldInformation = {
        Depth = { fieldType = "integer" },
        Color = { fieldType = "color" },
        Color2 = { fieldType = "color" },
    },
    ignoredFields = function(entity)
        populate(entity)
        return {"_name", "_id", "originX", "originY"}
    end,
}
