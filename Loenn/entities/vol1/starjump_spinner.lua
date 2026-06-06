return {
    name = "ScugHelper/StarjumpSpinner",
    depth = -8500,
    placements = {
        {
            name = "normal",
            data = {
                AttachToSolid = false,
                SpritePath = "danger/crystal"
            }
        }
    },

    texture = function(room, entity) return entity.SpritePath .. "/fg_white00" end,
}
