return {
    name = "ScugHelper/DashAngleRestrictionTrigger",
    depth = -100,
    placements = {
        { name = "cardinal", data = { CoverRoom = false, width = 32, height = 32, Angles = "0,90,180,270", Flag = "", FlagState = false } },
        { name = "diagonal", data = { CoverRoom = false, width = 32, height = 32, Angles = "45,135,225,315", Flag = "", FlagState = false } },
        { name = "starfish", data = { CoverRoom = false, width = 32, height = 32, Angles = "90,162,234,306,18", Flag = "", FlagState = false } },
        { name = "custom",   data = { CoverRoom = false, width = 32, height = 32, Angles = "0", Flag = "", FlagState = false } },
    },
    fieldInformation = {
        Angles = {
            fieldType = "list",
            elementDefault = "0",
            elementSeparator = ",",
            minimumElements = 1
        }
    },
    fillColor = { 0, 0, 0, 0 },
    outlineColor = { 1, 1, 1 }
}
