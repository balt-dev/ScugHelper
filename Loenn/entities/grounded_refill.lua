local drawableSprite = require("structs.drawable_sprite")

local refill = {}

refill.name = "ScugHelper/GroundedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_dash",
        alternativeName = "one_dash_crystal",
        data = {
            twoDash = false
        }
    },
    {
        name = "two_dashes",
        alternativeName = "two_dashes_crystal",
        data = {
            twoDash = true
        }
    }
}

function refill.sprite(room, entity)
    local base = drawableSprite.fromTexture((entity.twoDash and "objects/refillTwo/idle00") or "objects/refill/idle00", entity)
    local underline = drawableSprite.fromTexture("objects/groundedRefill/underline", entity)
    return { base, underline }
end

return refill
