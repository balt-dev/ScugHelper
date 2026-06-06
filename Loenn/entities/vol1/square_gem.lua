local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/SquareGem",
    depth = -15,
    placements = {
        {
            name = "normal",
            data = { Color = "a7fff2", Gravity = false },
        }
    },
    sprite = function(room, entity)
        local base = drawableSprite.fromTexture("objects/ScugHelper/squareGem/gemBase00", entity)
        local outline = drawableSprite.fromTexture("objects/ScugHelper/squareGem/gemOutline00", entity)
        base:setColor(scughelper.parseColor(entity.Color))
        outline:setColor({1, 1, 1})
        return { base, outline }
    end,
    fieldInformation = {
        Color = { fieldType = "color" },
    }
}
