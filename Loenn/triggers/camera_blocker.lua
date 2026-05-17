local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/CameraBlocker",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = { width = 32, height = 32, Flag = "", State = true, Priority = 0 }
        },
    },
    fieldInformation = { Flag = scughelper.builtinFlags },
    fillColor = { 1, 0, 0, 0.2 },
    outlineColor = { 1, 0.5, 0.5 }
}
