local drawableSprite = require("structs.drawable_sprite")
return {
    name = "ScugHelper/StarjumpSpinner",
    depth = -8500,
    placements = {
        {
            name = "normal",
            data = {
                AttachToSolid = false,
                SpritePath = "danger/crystal"
            }
        }
    },
    sprite = function(room, entity)
        local base = drawableSprite.fromTexture(entity.SpritePath .. "/fg_white00", entity)
        base:setColor({1, 1, 1, 0.15})
        return { base }
    end
}
