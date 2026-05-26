-- LUA GATE
--
-- BOTTOM TEXT

local utils = require("utils")
local drawing = require("utils.drawing")
local drawableText = require("structs.drawable_text")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

return {
    name = "ScugHelper/SandboxedLuaGate",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {Angle = 0, Size = 32, __eula = false, Arguments = "", FilePath = "", InlineLua = ""}
        },
    },
    ignoredFields = function(entity)
        if entity.__eula == nil then entity.__eula = false end
        if entity.__eula or scughelper.eulaAccepted then
            entity.__eula = false
            scughelper.eulaAccepted = true
            return {"_name", "_id", "originX", "originY", "__eula"}
        else
            return {"_name", "_id", "originX", "originY", "Arguments", "FilePath", "InlineLua", "x", "y", "Angle", "Size", "Immediate", "Groups", "Delay"}
        end
    end,
    sprite = function(room, entity)
        return {
            scughelper.drawableGate(entity.x, entity.y, entity.Angle, entity.Size, scughelper.colors "luaAction"),
            drawableText.fromText(
                (#entity.InlineLua > 0 and entity.InlineLua:gsub(";", "\n")) or entity.FilePath,
                entity.x + 2, entity.y + 2, nil, nil, nil, 0.25
            ),
        }
    end,
    selection = function(room, entity)
        return utils.rectangle(
            entity.x - 8,
            entity.y - 8,
            16, 16
        )
    end,
    fieldInformation = {
        Axis = {
            type = "enum",
            options = { {"Horizontal", 0}, {"Vertical", 1}, {"Total", 2} }
        },
        Comparison = {
            type = "enum",
            options = { {"<", 0}, {"<=", 1}, {">", 2}, {">=", 3} }
        },
        Action = {
            type = "enum",
            options = { {"Kill", 0}, {"SetFlag", 1}, {"SetSpeed", 2} }
        }
    }
}
