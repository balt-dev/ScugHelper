local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

return {
    name = "ScugHelper/CustomDistortion",
    depth = function(room, entity) return -1e20 end,
    placements = {
        {
            name = "normal",
            data = {
                ImagePath = "rotSquare/distort",
                OverlayPath = "rotSquare/overlay",
                Framerate = 30,
                OverlayFramerate = 30
            }
        },
    },
    sprite = function(room, entity)
        local spr, overlaySpr
        for i = 0, 5 do
            spr = drawableSprite.fromTexture("distortions/" .. entity.ImagePath .. ("0"):rep(i), entity)
            if spr then break end
        end
        if not spr then
            spr = drawableSprite.fromInternalTexture("missing_image")
        end
        spr.x = entity.x
        spr.y = entity.y
        spr.justificationX = 0
        spr.justificationY = 0
        spr:setColor { 1, 1, 1, 0.3 }
        if entity.OverlayPath and entity.OverlayPath ~= "" then
            for i = 0, 5 do
                overlaySpr = drawableSprite.fromTexture("distortions/" .. entity.OverlayPath .. ("0"):rep(i), entity)
                if overlaySpr then break end
            end
            if not overlaySpr then
                overlaySpr = drawableSprite.fromInternalTexture("missing_image")
            end
            overlaySpr.x = entity.x
            overlaySpr.y = entity.y
            overlaySpr.justificationX = 0
            overlaySpr.justificationY = 0
        end
        return { spr, overlaySpr }
    end,
}
