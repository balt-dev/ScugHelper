local effect = {}

effect.name = "ScugHelper/Balatro"
effect.canBackground = true
effect.canForeground = true

effect.defaultData = {
    SpinEase = 0.5,
    Contrast = 1,
    SpinAmount = 1,
    TimeScale = 1,
    SpinTimeScale = 0.73,
    OffsetX = 0,
    OffsetY = 0,
    Color1 = "#FF0000",
    Opacity1 = 1,
    Color2 = "#000000",
    Opacity2 = 0,
    Color3 = "#0000FF",
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
