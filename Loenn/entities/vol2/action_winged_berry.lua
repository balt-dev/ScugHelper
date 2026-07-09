local scughelper = require("mods").requireFromPlugin("libraries.scughelper")
local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")

local populate = scughelper.populateDefaults {
    moon = false,
    checkpointID = -1,
    order = -1,
    Groups = ""
}

return {
    name = "ScugHelper/ActionWingedBerry",
    depth = -100,
    fieldInformation = {
        order = {fieldType = "integer"},
        checkpointID = {fieldType = "integer"},
        Groups = { fieldType = "list", elementOptions = scughelper.actionGroups, elementDefault = "" },
    },
    placements = {
        {
            name = "jump",
            data = populate {Groups = "#PlayerJump,#PlayerSuperJump,#PlayerWallJump,#PlayerSuperWallJump,#PlayerClimbJump"},
        },
        {
            name = "dash",
            data = populate {Groups = "#PlayerDash" },
        },
        {
            name = "grab",
            data = populate {Groups = "#PlayerGrab" },
        },
        {
            name = "land",
            data = populate {Groups = "#PlayerLand" },
        },
        {
            name = "custom",
            data = populate {},
        },
    },
    nodeLineRenderType = "fan",
    nodeLimits = { 0, -1 },
    sprite = function(room, entity)
        local berryTex
        if entity.moon then
            berryTex = "collectables/moonBerry/ghost00"
        elseif entity.nodes and #entity.nodes > 0 then
            berryTex = "collectables/ghostberry/wings01"
        else
            berryTex = "collectables/strawberry/wings01"
        end
        local berrySprite = drawableSprite.fromTexture(berryTex, entity)
        return {
            berrySprite,
            drawableText.fromText(entity.Groups:gsub(",", "\n"), entity.x - 6.25, entity.y - 6, nil, nil, nil, 0.25, {0, 0, 0, 1}),
            drawableText.fromText(entity.Groups:gsub(",", "\n"), entity.x - 6, entity.y - 6.25, nil, nil, nil, 0.25, {0, 0, 0, 1}),
            drawableText.fromText(entity.Groups:gsub(",", "\n"), entity.x - 6, entity.y - 5.75, nil, nil, nil, 0.25, {0, 0, 0, 1}),
            drawableText.fromText(entity.Groups:gsub(",", "\n"), entity.x - 5.75, entity.y - 6, nil, nil, nil, 0.25, {0, 0, 0, 1}),
            drawableText.fromText(entity.Groups:gsub(",", "\n"), entity.x - 6, entity.y - 6, nil, nil, nil, 0.25),
        }
    end,
    nodeTexture = function(room, entity)
        if entity.nodes and #entity.nodes > 0 then
            return "collectables/strawberry/seed00"
        end
    end,
}
