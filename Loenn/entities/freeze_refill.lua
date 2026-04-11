return {
    name = "ScugHelper/FreezeRefill",
    depth = -100,
    placements = {
        {
            name = "normal",
            data = {
                oneUse = false
            }
        }
    },
    texture = "objects/freezeRefill/idle00",
    selection = function(room, entity)
        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    end,
}
