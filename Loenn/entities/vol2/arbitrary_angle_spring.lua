local drawableSprite = require("structs.drawable_sprite")

return {
    name = "ScugHelper/ArbitraryAngleSpring",
    depth = -8501,
    sprite = function(room, entity)
        local spring = drawableSprite.fromTexture(entity.SpritePath .. "/00", entity)
        spring:setJustification(0.5, 1.0)
        spring.rotation = math.rad(entity.Angle)
        return { spring }
    end,
    placements = {
        name = "normal",
        data = {
            playerCanUse = true,
            NoDashRefill = false,
            NoStaminaRefill = false,
            NoBackCollide = true,
            BounceStrength = 240,
            SpritePath = "objects/spring",
            Angle = 0
        }
    }
}