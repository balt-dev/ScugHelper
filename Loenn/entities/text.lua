local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/Text",
    depth = function(room, entity) return entity.Depth end,
    placements = {
        {
            name = "normal",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, UpdateFrequency = 0, Value = "Hello, {playerName:}!", Depth = 10, Flag = "", InvertFlag = false, FontTexture = "objects/ScugHelper/text/smallFont"}
        },
        {
            name = "tiny",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, UpdateFrequency = 0, Value = "Hello, {playerName:}!", Depth = 10, Flag = "", InvertFlag = false, FontTexture = "objects/ScugHelper/text/tinyFont"}
        },
        {
            name = "techno",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, UpdateFrequency = 0, Value = "Hello, {playerName:}!", Depth = 10, Flag = "", InvertFlag = false, FontTexture = "objects/ScugHelper/text/technoFont"}
        },
        {
            name = "blocky",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, UpdateFrequency = 0, Value = "Hello, {playerName:}!", Depth = 10, Flag = "", InvertFlag = false, FontTexture = "objects/ScugHelper/text/blockyFont"}
        },
        {
            name = "custom",
            data = {Infill = "FFFFFF", Outline = "000000", DrawOutline = false, UpdateFrequency = 0, Value = "Hello, {playerName:}!", Depth = 10, Flag = "", InvertFlag = false, FontTexture = ""}
        },
    },
    sprite = function(room, entity)
        local text = entity.Value:gsub("\\\\", "\x80"):gsub("\\{", "{"):gsub("\\}", "}"):gsub("\\n", "\n"):gsub("\x80", "\\")
        local atlasSprite = drawableSprite.fromTexture(entity.FontTexture or "objects/ScugHelper/text/smallFont", entity)
        local _, _, atlasW, atlasH = atlasSprite:getRectangleRaw()
        local glyphWidth = math.floor(atlasW / 16)
        local glyphHeight = math.floor(atlasH / 6)
        local glyphs = {}
        local function drawText(color, xo, yo)
            local x = 0
            local y = 0
            for char in string.gmatch(text, ".") do
                if char == '\n' then
                    y = y + 1
                    x = 0
                else
                    local atlasIndex = string.byte(char) - 32
                    local atlasX = atlasIndex % 16
                    local atlasY = math.floor(atlasIndex / 16)
                    local localSprite = drawableSprite.fromTexture(entity.FontTexture or "objects/ScugHelper/text/smallFont", entity)
                    localSprite:setColor(color or {1, 1, 1})
                    localSprite:useRelativeQuad(atlasX * glyphWidth, atlasY * glyphHeight, glyphWidth - 1, glyphHeight - 1, true, true)
                    localSprite.x = entity.x + x * glyphWidth + xo
                    localSprite.y = entity.y + y * glyphHeight + yo
                    entity.maxTextW = math.max(entity.maxTextW or 0, (x + 1) * glyphWidth)
                    entity.maxTextH = math.max(entity.maxTextH or 0, (y + 1) * glyphHeight)
                    glyphs[#glyphs + 1] = localSprite
                    x = x + 1
                end
            end
        end
        if entity.DrawOutline then
            local outlineColor = scughelper.parseColor(entity.Outline)
            drawText(outlineColor, -1, -1)
            drawText(outlineColor, -1,  0)
            drawText(outlineColor, -1,  1)
            drawText(outlineColor,  0, -1)
            drawText(outlineColor,  0,  1)
            drawText(outlineColor,  1, -1)
            drawText(outlineColor,  1,  0)
            drawText(outlineColor,  1,  1)
        end
        local infillColor = scughelper.parseColor(entity.Infill)
        drawText(infillColor, 0, 0)
        return glyphs
    end,
    ignoredFields = function(entity)
        if entity.FontTexture == nil then entity.FontTexture = "objects/ScugHelper/text/smallFont" end
        if entity.RequiresUpdate ~= nil then
            entity.UpdateFrequency = (entity.RequiresUpdate and 0.01) or 0
            entity.RequiresUpdate = nil
        end
        return {"_name", "_id", "originX", "originY"}
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x - 1, entity.y - 1, (entity.maxTextW or 8) + 2, (entity.maxTextH or 8) + 2)
    end,
    fieldInformation = {
        Depth = { fieldType = "integer" },
        Infill = { fieldType = "color" },
        Outline = { fieldType = "color" },
        Flag = scughelper.builtinFlags
    }
}
