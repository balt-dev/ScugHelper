local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRect = require("structs.drawable_rectangle")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local populate = scughelper.populateDefaults {
    Tint = "000000",
    TintAlpha = 0.5,
    TintAlphaSlider = "",
    AlphaCoefficient = 0,
    OffsetX = 0,
    OffsetY = 2,
    ColorSourceBlend = "SourceAlpha",
    AlphaSourceBlend = "SourceAlpha",
    ColorDestinationBlend = "InverseSourceAlpha",
    AlphaDestinationBlend = "DestinationAlpha",
    ColorBlendFunction = "Add",
    AlphaBlendFunction = "Add",
}

local blendOptions = {
    {"1, 1, 1, 1", "One"},
    {"0, 0, 0, 0", "Zero"},
    {"src.R, src.G, src.B, src.A", "SourceColor"},
    {"1 - src.R, 1 - src.G, 1 - src.B, 1 - src.A", "InverseSourceColor"},
    {"src.A, src.A, src.A, src.A", "SourceAlpha"},
    {"1 - src.A, 1 - src.A, 1 - src.A, 1 - src.A", "InverseSourceAlpha"},
    {"dst.R, dst.G, dst.B, dst.A", "DestinationColor"},
    {"1 - dst.R, 1 - dst.G, 1 - dst.B, 1 - dst.A", "InverseDestinationColor"},
    {"dst.A, dst.A, dst.A, dst.A", "DestinationAlpha"},
    {"1 - dst.A, 1 - dst.A, 1 - dst.A, 1 - dst.A", "InverseDestinationAlpha"},
    {"max(src.A, 1 - src.A), max(src.A, 1 - src.A), max(src.A, 1 - src.A), 1", "SourceAlphaSaturation"},
}
local blendFuncOptions = {
    {"(srcColor * srcBlend) + (destColor * destBlend)", "Add"},
    {"(srcColor * srcBlend) - (destColor * destBlend)", "Subtract"},
    {"(destColor * destBlend) - (srcColor * srcBlend)", "ReverseSubtract"},
    {"max((srcColor * srcBlend), (destColor * destBlend))", "Max"},
    {"min((srcColor * srcBlend), (destColor * destBlend))", "Min"},
}

return {
    name = "ScugHelper/RimLightController",
    depth = -20000,
    placements = {
        {
            name = "composite",
            data = populate {}
        },
        {
            name = "multiply",
            data = populate {
                ColorSourceBlend = "DestinationColor",
                AlphaSourceBlend = "SourceAlpha",
                ColorDestinationBlend = "InverseSourceAlpha",
                AlphaDestinationBlend = "DestinationAlpha",
                AlphaCoefficient = 1
            }
        },
    },
    fieldOrder = {
        "x", "y",
        "ColorSourceBlend", "AlphaSourceBlend",
        "ColorDestinationBlend", "AlphaDestinationBlend",
        "ColorBlendFunction", "AlphaBlendFunction",
        "OffsetX", "OffsetY",
        "Tint", "TintAlpha",
        "TintAlphaSlider", "AlphaCoefficient"
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                "Rim Light\nController",
                entity.x - 27, entity.y - 12, 54, 24, nil, 1
            ),
            drawableRect.fromRectangle("line", entity.x - 27, entity.y - 12, 54, 24, { 1, 1, 1, 1 })
        }
    end,
    fieldInformation = {
        OffsetX = {fieldType = "integer"},
        OffsetY = { fieldType = "integer" },
        Tint = {fieldType = "color"},
        ColorSourceBlend = {options = blendOptions, editable = false },
        ColorDestinationBlend = {options = blendOptions, editable = false },
        AlphaSourceBlend = {options = blendOptions, editable = false },
        AlphaDestinationBlend = { options = blendOptions , editable = false },
        ColorBlendFunction = {options = blendFuncOptions, editable = false },
        AlphaBlendFunction = {options = blendFuncOptions, editable = false },
    },
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 27,
            entity.y - 12,
            54, 24
        )
    end
}
