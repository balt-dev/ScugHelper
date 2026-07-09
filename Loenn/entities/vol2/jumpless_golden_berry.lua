return {
    name = "ScugHelper/JumplessGoldenBerry",
    depth = -100,
    nodeLineRenderType = "fan",
    nodeLimits = {0, -1},
    texture = function (room, entity)
        if entity.nodes and #entity.nodes > 0 then
            return "collectables/ghostgoldberry/wings01"
        else
            return "collectables/goldberry/wings01"
        end
    end,
    nodeTexture = function (room, entity)
        if entity.nodes and #entity.nodes > 0 then
            return "collectables/goldberry/seed00"
        end
    end,
    placements = {
        {
            name = "golden",
            data = {},
        }
    }
}