local seekerBarrier = {}

seekerBarrier.name = "ScugHelper/MovingSeekerBarrier"
seekerBarrier.depth = 0
seekerBarrier.color = {0.25, 0.25, 0.25, 0.8}
seekerBarrier.nodeLimits = {1, 1}
seekerBarrier.placements = {
    name = "normal",
    alternativeName = "jelly",
    data = {
        width = 8,
        height = 8,
        Period = 2
    }
}


return seekerBarrier
