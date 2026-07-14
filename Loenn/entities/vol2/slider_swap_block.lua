local drawableSprite = require("structs.drawable_sprite")
local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")

local swapBlock = {}

local frameNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local frameNodeNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    color = { 1.0, 1.0, 1.0, 0.7 }
}

local trailNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    useRealSize = true
}

local pathNinePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

local pathDepth = 8999
local trailDepth = 8999
local blockDepth = -9999

swapBlock.name = "ScugHelper/SliderSwapBlock"
swapBlock.nodeLimits = { 1, 1 }
swapBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            Particles = true,
            Slippery = false,
            HidePath = false,
            HideBackground = false,
            HideMiddle = false,
            SpriteDirectory = "objects/swapblock",
            MoveSound = "event:/game/05_mirror_temple/swapblock_move",
            MoveEndSound = "event:/game/05_mirror_temple/swapblock_move_end",
            Slider = "",
            ClampToBounds = true,
            SpeedLimit = -1
        }
    }
}

swapBlock.warnBelowSize = { 16, 16 }

local function addBlockSprites(sprites, entity, position, frameTexture, middleTexture, isNode)
    local x, y = position.x or 0, position.y or 0
    local width, height = entity.width or 8, entity.height or 8

    local ninePatchOptions = isNode and frameNodeNinePatchOptions or frameNinePatchOptions
    local frameNinePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local middleSprite = drawableSprite.fromTexture(middleTexture, position)
    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    middleSprite.depth = blockDepth

    if isNode then
        middleSprite:setColor({ 1, 1, 1, 0.7 })
    end

    for _, sprite in ipairs(frameSprites) do
        sprite.depth = blockDepth

        table.insert(sprites, sprite)
    end
end

local function addTrailSprites(sprites, entity, trailTexture, path)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 8, entity.height or 8
    local drawWidth, drawHeight = math.abs(x - nodeX) + width, math.abs(y - nodeY) + height

    x, y = math.min(x, nodeX), math.min(y, nodeY)

    if path then
        local pathDirection = x == nodeX and "V" or "H"
        local pathTexture = string.format(entity.SpriteDirectory .. "/path%s", pathDirection)
        local pathNinePatch = drawableNinePatch.fromTexture(pathTexture, pathNinePatchOptions, x, y, drawWidth,
            drawHeight)
        local pathSprites = pathNinePatch:getDrawableSprite()

        for _, sprite in ipairs(pathSprites) do
            sprite.depth = pathDepth

            table.insert(sprites, sprite)
        end
    end

    if (not entity.HideBackground) then
        local frameNinePatch = drawableNinePatch.fromTexture(trailTexture, trailNinePatchOptions, x, y, drawWidth,
            drawHeight)
        local frameSprites = frameNinePatch:getDrawableSprite()

        for _, sprite in ipairs(frameSprites) do
            sprite.depth = trailDepth

            table.insert(sprites, sprite)
        end
    end
end

function swapBlock.sprite(room, entity)
    local sprites = {}

    addTrailSprites(sprites, entity, entity.SpriteDirectory .. "/target", not entity.HidePath)
    addBlockSprites(sprites, entity, entity, entity.SpriteDirectory .. "/block",
        entity.SpriteDirectory .. "/midBlockRed00")

    return sprites
end

function swapBlock.nodeSprite(room, entity, node)
    local sprites = {}

    addBlockSprites(sprites, entity, entity, entity.SpriteDirectory .. "/block",
        entity.SpriteDirectory .. "/midBlockRed00", true)

    return sprites
end

function swapBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 8, entity.height or 8

    return utils.rectangle(x, y, width, height), { utils.rectangle(nodeX, nodeY, width, height) }
end

return swapBlock
