local drawableSprite = require("structs.drawable_sprite")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/GPUSpinner",
    depth = -8500,
    placements = {
        {
            name = "normal",
            data = {
                SpritePath = "danger/crystal",
                SpriteSuffix = "_white",
                Color = "FFFFFF",
                Rainbow = false
            }
        }
    },
    sprite = function(room, entity)
        local base = drawableSprite.fromTexture(entity.SpritePath .. "/fg" .. entity.SpriteSuffix .. "00", entity)
        base:setColor(scughelper.parseColor(entity.Color))
        return { base }
    end
}
