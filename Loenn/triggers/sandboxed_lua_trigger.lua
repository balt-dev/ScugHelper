local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/SandboxedLuaTrigger",
    depth = -100,
    placements = {
        {
            name = "trigger",
            data = {width = 32, height = 32, __eula = false, Arguments = "", FilePath = ""}
        },
    },
    ignoredFields = function(entity)
        if entity.__eula == nil then entity.__eula = false end
        if entity.__eula or scughelper.eulaAccepted then
            entity.__eula = false
            scughelper.eulaAccepted = true
            return {"_name", "_id", "originX", "originY", "__eula"}
        else
            return {"_name", "_id", "originX", "originY", "Arguments", "FilePath", "InlineLua", "x", "y", "width", "height"}
        end
    end,
    fieldInformation = {
        Arguments = {
            fieldType = "list",
            elementDefault = "nil"
        },
    },
    fieldOrder = {"__eula"},
    fillColor = {0, 0, 0, 0},
    outlineColor = {1, 1, 1}
}
