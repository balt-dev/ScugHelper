local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSpriteStruct = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local drawing = require("utils.drawing")

local templeEye = {}

templeEye.name = "ScugHelper/ProximityTempleEye"
templeEye.placements = {
    name = "normal", data = { trackPlayer = false, Radius = 80 }
}
templeEye.fieldInformation = { flag = scughelper.builtinFlags }


local function isBackground(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local tx = math.floor(x / 8) + 1
    local ty = math.floor(y / 8) + 1

    return room.tilesFg.matrix:get(tx, ty, "0") == "0"
end

function templeEye.depth(room, entity, viewport)
    return isBackground(room, entity) and 8990 or -10001
end

function templeEye.sprite(room, entity)
    local layer = isBackground(room, entity) and "bg" or "fg"

    local eyeSprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/" .. layer .. "_eye", entity)
    local lidSprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/" .. layer .. "_lid01", entity)

    return {
        eyeSprite,
        lidSprite,
        drawableFunc.fromFunction(function()
            drawing.callKeepOriginalColor(function()
                love.graphics.setColor { 1, 0, 1, 0.1 }
                love.graphics.circle("line", entity.x, entity.y, entity.Radius);
            end)
        end)
    }
end

function templeEye.selection(room, entity)
    -- Same size, just need selection
    local sprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/bg_eye", entity)

    return sprite:getRectangle()
end

return templeEye
