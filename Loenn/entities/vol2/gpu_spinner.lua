local drawableSprite = require("structs.drawable_sprite")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/GPUSpinner",
    depth = -8500,
    placements = {
        {
            name = "custom",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_white",
                Color = "FFFFFF",
                Rainbow = false
            }
        },
        {
            name = "red",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_red",
                Color = "FFFFFF",
                Rainbow = false
            }
        },
        {
            name = "purple",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_purple",
                Color = "FFFFFF",
                Rainbow = false
            }
        },
        {
            name = "blue",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_blue",
                Color = "FFFFFF",
                Rainbow = false
            }
        },
        {
            name = "rainbow",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_white",
                Color = "FFFFFF",
                Rainbow = true
            }
        }
    },
    fieldInformation = {
        Color = { fieldType = "color" },
    },
    sprite = function(room, entity)
        local base = drawableSprite.fromTexture(entity.SpritePath .. "/fg" .. entity.SpriteSuffix .. "00", entity)
        base:setColor(scughelper.parseColor(entity.Color))
        return { base }
    end
}
