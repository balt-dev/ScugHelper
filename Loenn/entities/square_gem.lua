local drawableSprite = require("structs.drawable_sprite")

local function getColor(color)
    local success, r, g, b = utils.parseHexColor(color)
    return success and { r, g, b } or { 1, 1, 1 }
end

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
        local base = drawableSprite.fromTexture("objects/squareGem/gemBase00", entity)
        local outline = drawableSprite.fromTexture("objects/squareGem/gemOutline00", entity)
        base:setColor(getColor(entity.Color))
        outline:setColor({1, 1, 1})
        return { base, outline }
    end,
    fieldInformation = {
        Color = { fieldType = "color" },
    }
}
