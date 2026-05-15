local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local cassetteBlock = {}

cassetteBlock.name = "ScugHelper/ActionCassetteBlock"
cassetteBlock.warnBelowSize = { 16, 16 }
cassetteBlock.placements = {
    {
        name = "jump",
        data = { Silent = false, width = 16, height = 16, Groups = "#PlayerJump", PressedTexture = "objects/cassetteblock/pressed00", SolidTexture = "objects/cassetteblock/solid", Color = "49aaf0", StartSolid = true, SurfaceSoundIndex = 35 }
    },
    {
        name = "dash",
        data = { Silent = false, width = 16, height = 16, Groups = "#PlayerDash", PressedTexture = "objects/cassetteblock/pressed00", SolidTexture = "objects/cassetteblock/solid", Color = "49aaf0", StartSolid = true, SurfaceSoundIndex = 35 }
    },
    {
        name = "grab",
        data = { Silent = false, width = 16, height = 16, Groups = "#PlayerGrab", PressedTexture = "objects/cassetteblock/pressed00", SolidTexture = "objects/cassetteblock/solid", Color = "49aaf0", StartSolid = true, SurfaceSoundIndex = 35 }
    },
    {
        name = "action",
        data = { Silent = false, width = 16, height = 16, Groups = "", PressedTexture = "objects/cassetteblock/pressed00", SolidTexture = "objects/cassetteblock/solid", Color = "49aaf0", StartSolid = true, SurfaceSoundIndex = 35 }
    },
    {
        name = "flag",
        data = { Silent = false, width = 16, height = 16, Flag = "ScugHelper.PlayerAirborne", FlagState = true, PressedTexture = "objects/cassetteblock/pressed00", SolidTexture = "objects/cassetteblock/solid", Color = "49aaf0", SurfaceSoundIndex = 35 }
    }
}
cassetteBlock.fieldInformation = {
    Color = { fieldType = "color" },
    Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" }
}

-- Filter by cassette blocks sharing the same index
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.Groups == target.Groups and entity.Flag == target.Flag and
            entity.FlagState == target.FlagState and entity.StartSolid == target.StartSolid
    end
end

local function getTileSprite(entity, x, y, frame, color, depth, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0
        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8
        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16
        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24
        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0
        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16
        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8
        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8
        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0
        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0
        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16
        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(color)

        sprite.depth = depth

        return sprite
    end
end

function cassetteBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local color = scughelper.parseColor(entity.Color)
    local frame = (entity.StartSolid and entity.SolidTexture) or entity.PressedTexture
    local depth = -10

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, color, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return cassetteBlock
