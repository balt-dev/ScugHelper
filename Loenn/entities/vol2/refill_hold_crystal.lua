local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

return {
    name = "ScugHelper/RefillHoldCrystal",
    depth = -20,
    placements = {
        {
            name = "green",
            data = {
                BackgroundColor = "005000",
                ParticleColors = "003a00,93bd40,8fcb5e,ffffff",
                FallbackRefill = "OneDash"
            }
        },
        {
            name = "pink",
            data = {
                BackgroundColor = "821d61",
                ParticleColors = "6b124e,fb94ff,ffb6f5,ffffff",
                FallbackRefill = "TwoDash"
            }
        },
        {
            name = "blue",
            data = {
                BackgroundColor = "30309d",
                ParticleColors = "132153,5e70f6,8d99e5,ffffff",
                FallbackRefill = "Midair"
            }
        },
        {
            name = "cyan",
            data = {
                BackgroundColor = "306075",
                ParticleColors = "112c42,a5cdff,7bb5e5,ffffff",
                FallbackRefill = "Overcharge"
            }
        },
        {
            name = "black",
            data = {
                BackgroundColor = "000000",
                ParticleColors = "202020,606060,a0a0a0,ffffff",
                FallbackRefill = "Limbo"
            }
        },
        {
            name = "dream",
            data = {
                BackgroundColor = "000000",
                ParticleColors = "FFEF11,FF00D0,08a310,5fcde4,7fb25e,E0564C,5b6ee1,CC3B3B,7daa64",
                FallbackRefill = "DreamTunnel"
            }
        }
    },
    sprite = function(room, entity)
        local background = drawableSprite.fromTexture("objects/ScugHelper/dreamCrystal/background", entity)
        local overlay = drawableSprite.fromTexture("objects/ScugHelper/dreamCrystal/overlay", entity)
        local backgroundColor = scughelper.parseColor(entity.BackgroundColor)
        background:setColor(backgroundColor)
        background.renderOffsetY = 10
        overlay.renderOffsetY = 10
        return { background, overlay }
    end,
    selection = function(room, entity)
        return utils.rectangle(entity.x - 10, entity.y - 20, 19, 20)
    end,
}
