local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableRect = require("structs.drawable_rectangle")

return {
    name = "ScugHelper/ProceduralTilemap",
    depth = -1000000,
    placements = {
        {
            name = "static",
            data = { width = 64, height = 64, FilePath = "Assets/ProceduralTilemap/ScugHelper/staticExample", Arguments = "", MoveWithPlayer = false }
        },
        {
            name = "followPlayer",
            data = { width = 640, height = 480, FilePath = "Assets/ProceduralTilemap/ScugHelper/followExample", Arguments = "", MoveWithPlayer = true }
        },
    },
    ignoredFields = function(entity)
        if entity.__eula == nil then entity.__eula = false end
        if entity.__eula or scughelper.eulaAccepted then
            entity.__eula = false
            scughelper.eulaAccepted = true
            if (entity.MoveWithPlayer) then
                return {"_name", "_id", "originX", "originY", "__eula", "MoveWithPlayer", "width", "height"}
            else
                return {"_name", "_id", "originX", "originY", "__eula", "MoveWithPlayer"}
            end
        else
            return {"_name", "_id", "originX", "originY", "Arguments", "FilePath", "MoveWithPlayer", "x", "y", "width", "height", "Seed"}
        end
    end,
    fieldInformation = {
        Arguments = {
            fieldType = "list",
            elementDefault = "nil"
        },
        Seed = { fieldType = "integer" }
    },
    sprite = function(room, entity)
        if (entity.MoveWithPlayer) then
            entity.width = 40 * 4 * 8
            entity.height = 23 * 4 * 8
        end
        return {
            drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, entity.height, {0, 1, 1, 1}),
            (entity.MoveWithPlayer and drawableRect.fromRectangle("line", entity.x, entity.y, 40 * 8, entity.height, {1, 1, 0, 1})) or nil,
            (entity.MoveWithPlayer and drawableRect.fromRectangle("line", entity.x + entity.width - 40 * 8, entity.y, 40 * 8, entity.height, {1, 1, 0, 1})) or nil,
            (entity.MoveWithPlayer and drawableRect.fromRectangle("line", entity.x, entity.y, entity.width, 23 * 8, {1, 1, 0, 1})) or nil,
            (entity.MoveWithPlayer and drawableRect.fromRectangle("line", entity.x, entity.y + entity.height - 23 * 8, entity.width, 23 * 8, {1, 1, 0, 1})) or nil,
        }
    end
}
