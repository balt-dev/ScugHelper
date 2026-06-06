local drawableSprite = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")
local utils = require("utils")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

local templeGate = {}

local textures = {
    default = "objects/door/TempleDoor00",
    mirror = "objects/door/TempleDoorB00",
    theo = "objects/door/TempleDoorC00"
}

local textureOptions = {}
local typeOptions = celesteEnums.temple_gate_modes

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

templeGate.name = "ScugHelper/ActionTempleGate"
templeGate.depth = -9000
templeGate.canResize = {false, false}
templeGate.fieldInformation = {
    sprite = {
        options = textureOptions,
        editable = false
    },
    type = {
        options = typeOptions,
        editable = false
    },
    OpenGroups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""},
    CloseGroups = {fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = ""},
}
templeGate.placements = {
    {
        name = "normal",
        placementType = "point",
        data = {
            height = 48,
            sprite = "default",
            OpenGroups = "",
            CloseGroups = "",
            startOpen = false
        }
    }
}

function templeGate.sprite(room, entity)
    local variant = entity.sprite or "default"
    local texture = textures[variant] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)
    local height = entity.height or 48

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, height - 48)

    return sprite
end

return templeGate
