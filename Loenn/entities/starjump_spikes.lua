local entities = require("entities")

table.insert(entities.registeredEntities.spikesUp.placements, {
    name = "ScugHelper/StarjumpSpikes-Up",
    associatedMods = {"ScugHelper"},
    data = {
        width = 8,
        ["type"] = "ScugHelper/starjump"
    }
})

table.insert(entities.registeredEntities.spikesDown.placements, {
    name = "ScugHelper/StarjumpSpikes-Down",
    associatedMods = {"ScugHelper"},
    data = {
        width = 8,
        ["type"] = "ScugHelper/starjump"
    }
})

table.insert(entities.registeredEntities.spikesLeft.placements, {
    name = "ScugHelper/StarjumpSpikes-Left",
    associatedMods = {"ScugHelper"},
    data = {
        height = 8,
        ["type"] = "ScugHelper/starjump"
    }
})


table.insert(entities.registeredEntities.spikesRight.placements, {
    name = "ScugHelper/StarjumpSpikes-Right",
    associatedMods = {"ScugHelper"},
    data = {
        height = 8,
        ["type"] = "ScugHelper/starjump"
    }
})

return {}