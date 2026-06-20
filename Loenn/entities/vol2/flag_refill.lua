local utils = require("utils")
local drawableText = require("structs.drawable_text")
local scughelper = require("mods").requireFromPlugin("libraries.scughelper")

local populate = scughelper.populateDefaults {
    oneUse = false,
    Flag = "",
    SpritePath = "objects/ScugHelper/flagRefill",
    FlagState = true,
    ParticleColor1 = "FFFFFF",
    ParticleColor2 = "A0A0A0",
    RespawnTimer = 2.5
}

return {
    name = "ScugHelper/FlagRefill",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = populate {}
        }
    },
    texture = function(room, entity)
        populate(entity)
        return entity.SpritePath .. "/idle00"
    end,
    selection = function(room, entity)
        populate(entity)
        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    end,
    fieldInformation = {
        ParticleColor1 = { fieldType = "color" },
        ParticleColor2 = { fieldType = "color" },
        Flag = scughelper.builtinFlags
    }
}
