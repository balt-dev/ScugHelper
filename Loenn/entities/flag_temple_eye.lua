local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSpriteStruct = require("structs.drawable_sprite")

local templeEye = {}

templeEye.name = "ScugHelper/FlagTempleEye"
templeEye.placements = {
    name = "normal", data = { trackPlayer = false, flag = "" }
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

    return {eyeSprite, lidSprite}
end

function templeEye.selection(room, entity)
    -- Same size, just need selection
    local sprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/bg_eye", entity)

    return sprite:getRectangle()
end

return templeEye
