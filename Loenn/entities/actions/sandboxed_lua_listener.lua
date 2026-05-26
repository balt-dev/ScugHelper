local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local utils = require("utils")
local drawableText = require("structs.drawable_text")

return {
    name = "ScugHelper/SandboxedLuaListener",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = {width = 32, height = 24, __eula = false, Arguments = "", FilePath = "", InlineLua = "", Immediate = false, Groups = "", Delay = 0, Invert = true, Targets = ""}
        },
    },
    ignoredFields = function(entity)
        if entity.__eula == nil then entity.__eula = false end
        if entity.__eula or scughelper.eulaAccepted then
            entity.__eula = false
            scughelper.eulaAccepted = true
            return {"_name", "_id", "originX", "originY", "__eula"}
        else
            return {"_name", "_id", "originX", "originY", "Arguments", "FilePath", "x", "y", "width", "height", "Immediate", "Groups", "Delay", "InlineLua"}
        end
    end,
    fieldInformation = {
        Arguments = {
            fieldType = "list",
            elementDefault = "nil"
        },
        Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" },
        Targets = {fieldType = "list"}
    },
    sprite = function(room, entity)
        return {
            drawableText.fromText(
                ("Action: Lua Listener\n%s (%.3fs)\n%s\n%s"):format(entity.Groups, entity.Delay, (#entity.InlineLua > 0 and entity.InlineLua:gsub(";", "\n")) or entity.FilePath, entity.Arguments),
                entity.x + 2, entity.y + 2, nil, nil, nil, 0.25
            ),
            scughelper.unfuckedRect(entity, scughelper.colors "luaAction")
        }
    end,
    rectangle = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    end
}
