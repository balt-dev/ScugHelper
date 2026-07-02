local effect = {}

effect.name = "ScugHelper/Balatro"
effect.canBackground = true
effect.canForeground = true

-- new_colour = G.C.BLUE, special_colour = G.C.RED, tertiary_colour = darken(G.C.BLACK, 0.4), contrast = 3

effect.defaultData = {
    SpinEase = 0.5,
    Contrast = 3,
    SpinAmount = 1,
    TimeScale = 1,
    SpinTimeScale = 0.73,
    OffsetX = 0,
    OffsetY = 0,
    Color1 = "#FE5F55",
    Opacity1 = 0.4,
    Color2 = "#009dff",
    Opacity2 = 1,
    Color3 = "#030303",
    Opacity3 = 1,
    Tint = "#FFFFFF",
    TintOpacity = 1,
    Add = false
}

effect.fieldInformation = {
    Color1 = { fieldType = "color" },
    Color2 = { fieldType = "color" },
    Color3 = { fieldType = "color" },
    Tint = { fieldType = "color" },
}

effect.fieldOrder = {
    "only", "exclude", "tag", "flag",
    "notflag", "SpinEase", "Contrast", "SpinAmount",
    "Color1", "Opacity1", "Color2", "Opacity2",
    "Color3", "Opacity3", "Tint", "TintOpacity",
    "OffsetX", "OffsetY", "TimeScale", "SpinTimeScale",
    "Add"
}

return effect
