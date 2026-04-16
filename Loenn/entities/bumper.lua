local utils = require("utils")

return {
    name = "ScugHelper/RecoilBumper",
    depth = -20,
    placements = {
        {
            name = "normal",
            data = { Mass = 2, Drag = 200 },
        }
    },
    texture = "objects/recoilBumper/Idle00",
    selection = function(room, entity)
        return utils.rectangle(entity.x - 12, entity.y - 12, nil, nil)
    end,
}
