local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")

local clutterSwitch = {}

clutterSwitch.name = "ScugHelper/FlagClutterSwitch"
clutterSwitch.depth = 0
clutterSwitch.placements = {
    name = "normal",
    data = { Flag = "", Icon = "objects/resortclutter/icon_lightning", TargetState = true }
}

local buttonTexture = "objects/resortclutter/clutter_button00"

function clutterSwitch.sprite(room, entity)
    local buttonSprite = drawableSprite.fromTexture(buttonTexture, entity)
    local clutterSprite = drawableSprite.fromTexture(string.format(entity.Icon), entity)

    buttonSprite:setJustification(0.5, 1.0)
    buttonSprite:addPosition(16, 16)

    clutterSprite:setJustification(0.5, 0.5)
    clutterSprite:addPosition(16, 8)

    return {
        buttonSprite,
        clutterSprite,
        drawableText.fromText(
            entity.Flag,
            entity.x, entity.y - 8, entity.width, 8, nil, 0.5
        ),
    }
end

return clutterSwitch
