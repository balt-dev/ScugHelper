local drawableSprite = require("structs.drawable_sprite")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

local populate = scughelper.populateDefaults {
    SpritePath = "danger/crystal",
    SpriteSuffix = "_white",
    Color = "FFFFFF",
    ShatterColor = "FFFFFF",
    Rainbow = false,
    attachToSolid = false
}

return {
    name = "ScugHelper/GPUSpinner",
    depth = -8500,
    placements = {
        {
            name = "custom",
            data = {}
        },
        {
            name = "red",
            data = {
                SpriteSuffix = "_red",
                ShatterColor = "ff4f4f"
            }
        },
        {
            name = "purple",
            data = {
                SpriteSuffix = "_purple",
                ShatterColor = "ff4fef"
            }
        },
        {
            name = "blue",
            data = {
                SpriteSuffix = "_blue",
                ShatterColor = "639bff"
            }
        },
        {
            name = "rainbow",
            data = { Rainbow = true }
        }
    },
    fieldInformation = {
        ShatterColor = { fieldType = "color" },
        Color = { fieldType = "color" },
    },
    sprite = function(room, entity)
        entity.ShatterColor = entity.ShatterColor or entity.Color
        populate(entity)
        local base = drawableSprite.fromTexture(entity.SpritePath .. "/fg" .. entity.SpriteSuffix .. "00", entity)
        base:setColor(scughelper.parseColor(entity.Color))
        return { base }
    end
}
