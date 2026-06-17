local utf8 = require("utf8")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableText = require("structs.drawable_text")

local populate = scughelper.populateDefaults {
    Infill = "FFFFFF",
    Outline = "000000",
    OutlineType = 0,
    UpdateFrequency = 0,
    Value = "Hello, {playerName:}!",
    Depth = 10,
    Flag = "",
    InvertFlag = false,
    FontTexture = "objects/ScugHelper/text/normalFont",
    Opacity = 1,
    ParallaxX = 0,
    ParallaxY = 0,
    ParallaxOffsetX = 0,
    ParallaxOffsetY = 0,
    Persistent = false
}

return {
    name = "ScugHelper/Text",
    depth = function(room, entity) return entity.Depth end,
    placements = {
        {
            name = "default",
            data = populate { FontTexture = "objects/ScugHelper/text/normalFont" }
        },
        {
            name = "normal",
            data = populate { FontTexture = "objects/ScugHelper/text/smallFont" }
        },
        {
            name = "tile",
            data = populate { FontTexture = "objects/ScugHelper/text/tileFont" }
        },
        {
            name = "tiny",
            data = populate { FontTexture = "objects/ScugHelper/text/tinyFont" }
        },
        {
            name = "techno",
            data = populate { FontTexture = "objects/ScugHelper/text/technoFont" }
        },
        {
            name = "blocky",
            data = populate { FontTexture = "objects/ScugHelper/text/blockyFont" }
        },
    },
    sprite = function(room, entity)
        local text = entity.Value:gsub("\\\\", "\x80"):gsub("\\{", "{"):gsub("\\}", "}"):gsub("\\n", "\n"):gsub("\x80", "\\")
        local atlasSprite = drawableSprite.fromTexture(entity.FontTexture or "objects/ScugHelper/text/smallFont", entity)
        local _, _, atlasW, atlasH = atlasSprite:getRectangleRaw()
        local glyphWidth = math.floor(atlasW / 16)
        local glyphHeight = math.floor(atlasH / 6)
        entity.maxTextW = 0
        entity.maxTextH = 0
        local glyphs = {}
        local function drawText(color, xo, yo)
            local x = 0
            local y = 0
            for _, codepoint in utf8.codes(text) do
                if codepoint == 0x0A then
                    y = y + 1
                    x = 0
                else
                    if codepoint >= 32 then
                        if codepoint >= 127 then codepoint = 127 end
                        local atlasIndex = codepoint - 32
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
        end
        if entity.OutlineType == nil then entity.OutlineType = entity.DrawOutline and 1 or 0 end
        entity.DrawOutline = nil
        local outlineColor = scughelper.parseColor(entity.Outline)
        if ({true, false, false})[entity.OutlineType] then drawText(outlineColor, -1, -1) end
        if ({true, true, false})[entity.OutlineType] then drawText(outlineColor, -1,  0) end
        if ({true, false, false})[entity.OutlineType] then drawText(outlineColor, -1,  1) end
        if ({true, true, false})[entity.OutlineType] then drawText(outlineColor,  0, -1) end
        if ({true, true, false})[entity.OutlineType] then drawText(outlineColor,  0,  1) end
        if ({true, false, false})[entity.OutlineType] then drawText(outlineColor,  1, -1) end
        if ({true, true, false})[entity.OutlineType] then drawText(outlineColor,  1,  0) end
        if ({true, false, true})[entity.OutlineType] then drawText(outlineColor,  1,  1) end
        local infillColor = scughelper.parseColor(entity.Infill)
        drawText(infillColor, 0, 0)
        return glyphs
    end,
    ignoredFieldsMultiple = {"Value", "Depth", "Infill", "Outline", "OutlineType", "UpdateFrequency", "Flag", "InvertFlag"},
    ignoredFields = function(entity)
        if entity.OutlineType == nil then entity.OutlineType = entity.DrawOutline and 1 or 0 end
        entity.DrawOutline = nil
        if entity.RequiresUpdate ~= nil then
            entity.UpdateFrequency = (entity.RequiresUpdate and 0.01) or 0
            entity.RequiresUpdate = nil
        end
        populate(entity)
        return {"_name", "_id", "originX", "originY", "maxTextW", "maxTextH"}
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x - 1, entity.y - 1, (entity.maxTextW or 8) + 2, (entity.maxTextH or 8) + 2)
    end,
    fieldInformation = {
        Depth = { fieldType = "integer" },
        Infill = { fieldType = "color" },
        Outline = { fieldType = "color" },
        Flag = scughelper.builtinFlags,
        OutlineType = {fieldType = "integer", options = {{"None", 0}, {"Full", 1}, {"Edge", 2}, {"Drop Shadow", 3}}}
    },
    fieldOrder = {
        "x",
        "y",
        "Depth",
        "Opacity",
        "Infill",
        "Outline",
        "Value",
        "OutlineType",
        "UpdateFrequency",
        "FontTexture",
        "ParallaxX",
        "ParallaxY",
        "ParallaxOffsetX",
        "ParallaxOffsetY",
        "Flag",
        "InvertFlag",
        "Persistent",
    },
}
