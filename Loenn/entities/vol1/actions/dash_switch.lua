local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local textures = {
    default = "objects/temple/dashButton00",
    mirror = "objects/temple/dashButtonMirror00",
}
local textureOptions = {}

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

return {
    name = "ScugHelper/ActionDashSwitch",
    depth = 0,
    justification = { 0.5, 0.5 },
    fieldInformation = {
        sprite = {
            options = textureOptions
        },
        side = {
            options = {Up = 0, Down = 1, Left = 2, Right = 3}
        },
        Targets = {fieldType = "list"}
    },
    sprite = function(room, entity)
        local texture = entity.sprite == "default" and textures["default"] or textures["mirror"]
        local sprite = drawableSprite.fromTexture(texture, entity)

        if entity.side == 1 then
            sprite:addPosition(8, 8)
            sprite.rotation = math.pi / 2
        elseif entity.side == 0 then
            sprite:addPosition(8, 0)
            sprite.rotation = -math.pi / 2
        elseif entity.side == 3 then
            sprite:addPosition(8, 8)
            sprite.rotation = 0
        else
            sprite:addPosition(0, 8)
            sprite.rotation = math.pi
        end

        return sprite
    end,
    placements = {
        {name = "up", data = {Targets = "", side = 0, sprite = "default"}},
        {name = "down", data = {Targets = "", side = 1, sprite = "default"}},
        {name = "left", data = {Targets = "", side = 2, sprite = "default"}},
        {name = "right", data = {Targets = "", side = 3, sprite = "default"}},
    }
}
