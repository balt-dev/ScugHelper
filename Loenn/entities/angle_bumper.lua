local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/NewAngleBumper",
    depth = -100,
    nodeLimits = {1, 1},
    placements = {
        {
            name = "normal",
            data = {
                SpritePath = "objects/ScugHelper/newAngleBumper",
                DashCount = 1,
                LaunchSpeed = 280,
                RefillStamina = true,
                EmitParticles = true
            },
        }
    },
    sprite = function(room, entity)
        entity.nodes = entity.nodes or { x = entity.x + 16, y = entity.y }
        local deltaX = entity.nodes[1].x - entity.x
        local deltaY = entity.nodes[1].y - entity.y
        local deltaLength = math.sqrt(deltaX * deltaX + deltaY * deltaY)

        local angleVector = { X = deltaX / deltaLength, Y = deltaY / deltaLength }
        local rawAngleVector = { X = angleVector.X, Y = angleVector.Y }

        local flipX = false
        local flipY = false

        if angleVector.X < 0 then
            flipX = true
            angleVector.X = -angleVector.X
        end

        if angleVector.Y > 0
            then flipY = true
            else angleVector.Y = -angleVector.Y
        end

        local checkAngle = math.deg(math.acos(angleVector.X))

        local displayAngle
        if checkAngle < 10 then displayAngle = 0
        elseif checkAngle < 40 then displayAngle = 30
        elseif checkAngle < 80 then displayAngle = 60
        else displayAngle = 90
        end

        local middle = drawableSprite.fromTexture(entity.SpritePath .. "/middle22", entity)
        local edge = drawableSprite.fromTexture(entity.SpritePath .. "/angles/" .. displayAngle .. "_22", entity)
        if (flipX) then edge.scaleX = edge.scaleX * -1 end
        if (flipY) then edge.scaleY = edge.scaleY * -1 end
        middle.x = entity.x; middle.y = entity.y; edge.x = entity.x; edge.y = entity.y
        return {
            middle, edge,
            drawableFunc.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                    local targetX = entity.x + rawAngleVector.X * (entity.LaunchSpeed / 16);
                    local targetY = entity.y + rawAngleVector.Y * (entity.LaunchSpeed / 16);
                    love.graphics.setColor({ 0.5, 0.8, 1, 0.8 })
                    love.graphics.line(entity.x, entity.y, targetX, targetY);
                end)
            end),
        }
    end,
    nodeRectangle = function (room, entity, node, nodeIndex, viewport)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        local deltaX = nodeX - entity.x
        local deltaY = nodeY - entity.y
        local deltaLength = math.sqrt(deltaX * deltaX + deltaY * deltaY)

        local angleVector = { X = deltaX / deltaLength, Y = deltaY / deltaLength }
        local rawAngleVector = { X = angleVector.X, Y = angleVector.Y }

        local targetX = entity.x + rawAngleVector.X * (entity.LaunchSpeed / 16);
        local targetY = entity.y + rawAngleVector.Y * (entity.LaunchSpeed / 16);
        entity.nodes[1] = { x = targetX, y = targetY }
        return utils.rectangle((targetX or 0) - 2, (targetY or 0) - 2, 4, 4)
    end,
    nodeFillColor = {0, 0, 0, 0},
    nodeBorderColor = {1, 1, 1, 1},
    fieldInformation = { DashCount = {fieldType = "integer"}},
    selection = function(room, entity)
        local nodeX, nodeY = entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y
        local deltaX = nodeX - entity.x
        local deltaY = nodeY - entity.y
        local deltaLength = math.sqrt(deltaX * deltaX + deltaY * deltaY)

        local angleVector = { X = deltaX / deltaLength, Y = deltaY / deltaLength }
        local rawAngleVector = { X = angleVector.X, Y = angleVector.Y }

        local targetX = entity.x + rawAngleVector.X * (entity.LaunchSpeed / 16);
        local targetY = entity.y + rawAngleVector.Y * (entity.LaunchSpeed / 16);
        entity.nodes[1] = { x = targetX, y = targetY }

        return utils.rectangle(
            entity.x - 8,
            entity.y - 8,
            16, 16
        ), { utils.rectangle(entity.nodes[1].x - 2, entity.nodes[1].y - 2, 4, 4) }
    end
}
