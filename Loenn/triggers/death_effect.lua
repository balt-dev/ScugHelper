local drawableSprite = require("structs.drawable_sprite")
return {
    name = "ScugHelper/DeathEffectTrigger",
    depth = -100,
    nodeLineRenderType = "line",
    nodeLimits = { 1, 1 },
    placements = {
        {
            name = "normal",
            data = { width = 32, height = 32, Color = "FFFFFF", SpritePath = "characters/player/hair00", Amount = 8 }
        },
    },
    fieldInformation = { Color = { fieldType = "color" } },
    fillColor = { 0, 0, 0, 0 },
    outlineColor = { 1, 0.5, 0.5 },
    nodeSprite = function(room, entity, node) return { drawableSprite.fromTexture(entity.SpritePath, node) } end,
    nodeJustification = { 0.5, 0.5 }
}
