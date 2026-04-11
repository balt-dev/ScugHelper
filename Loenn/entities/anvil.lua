return {
    name = "ScugHelper/Anvil",
    depth = -20,
    placements = {
        {
            name = "normal",
            data = {}
        }
    },
    texture = "objects/anvil",
    selection = function(room, entity)
        return utils.rectangle(entity.x - 4, entity.y - 4, 8, 8)
    end,
}
