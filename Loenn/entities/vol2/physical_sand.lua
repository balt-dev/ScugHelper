local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSprite = require("structs.drawable_sprite")
local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

return {
    name = "ScugHelper/PhysicalSand",
    depth = function(room, entity) return (entity.Collidable and -9750) or 1750 end,
    placements = {
        {
            name = "normal",
            data = {
                width = 32,
                height = 32,
                CollisionUpdateFrequency = 0,
                UpdateFrequency = 0.05,
                RandomSeed = 0,
                InitialStateImage = "objects/citysatellite/dish",
                ImageOffsetX = 0,
                ImageOffsetY = 0,
                IfFlag = "",
                FlagState = true,
                Collidable = false,
                WrapX = false,
                WrapY = false,
                Range = 1,
                Tint = "FFFFFF"
            }
        },
    },
    fieldInformation = {
        RandomSeed = { fieldType = "integer" },
        ImageOffsetX = { fieldType = "integer" },
        ImageOffsetY = { fieldType = "integer" },
        Range = { fieldType = "integer" },
        Tint = {fieldType = "color"}
    },
    sprite = function(room, entity)
        local spr = drawableSprite.fromTexture(entity.InitialStateImage, entity)
        spr:setColor(scughelper.parseColor(entity.Tint))
        spr.x = entity.x + entity.ImageOffsetX
        spr.y = entity.y + entity.ImageOffsetY
        spr.justificationX = 0
        spr.justificationY = 0
        return {
            spr,
            drawableText.fromText(
                "Physical Sand",
                entity.x, entity.y, entity.width, entity.height
            ),
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, { 1, 1, 1, 1 })
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x,
            entity.y,
            entity.width,
            entity.height
        )
    end
}
