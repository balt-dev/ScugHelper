local effect = {}

effect.name = "ScugHelper/HallOfMirrors"
effect.canBackground = true
effect.canForeground = true

effect.defaultData = {
    OffsetX = 0,
    OffsetY = 0,
    SpeedX = 0,
    SpeedY = 0,
    FlipX = false,
    FlipY = false,
    ParallaxX = 0,
    ParallaxY = 0,
    Zoom = 1,
    Rotation = 0,
    RotationSpeed = 0,
    Color = "FFFFFF",
    Opacity = 1,
    CaptureForegroundAndBloom = false
}

effect.fieldInformation = {
    OffsetX = { fieldType = "integer" },
    OffsetY = { fieldType = "integer" },
    Color = { fieldType = "color" },
}

effect.fieldOrder = {
    "only", "exclude", "tag", "flag",
    "notflag", "Color", "Zoom", "Rotation",
    "OffsetX", "OffsetY", "SpeedX", "SpeedY",
    "ParallaxX", "ParallaxY", "RotationSpeed", "Opacity",
    "CaptureForegroundAndBloom", "FlipX", "FlipY"
}

return effect
