local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local oshiroDoor = {}

oshiroDoor.name = "ScugHelper/FlagClutterDoor"
oshiroDoor.depth = 0
oshiroDoor.fillColor = { 74 / 255, 71 / 255, 135 / 255, 153 }
oshiroDoor.borderColor = { 1.0, 1.0, 1.0, 1.0 }
oshiroDoor.placements = {
    name = "normal", data = { Flag = "", TargetState = true }
}
oshiroDoor.fieldInformation = { Flag = scughelper.builtinFlags }

local fillColor = { 74 / 255, 71 / 255, 135 / 255, 153 }
local borderColor = { 1.0, 1.0, 1.0, 1.0 }

function oshiroDoor.sprite(room, entity)
    local rectangle = utils.rectangle(entity.x, entity.y, 32, 32)
    local drawableRectangleSprites = drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor)
        :getDrawableSprite()

    table.insert(
        drawableRectangleSprites,
        drawableText.fromText(
            entity.Flag,
            entity.x + 2, entity.y + 2, 28, 28, nil, 0.5
        )
    )

    return drawableRectangleSprites
end

return oshiroDoor
