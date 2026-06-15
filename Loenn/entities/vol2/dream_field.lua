local dreamBlock = {}

dreamBlock.name = "ScugHelper/DreamField"
dreamBlock.fillColor = {0.0, 0.0, 0.0, 0.2}
dreamBlock.borderColor = {0, 0, 0, 0}
dreamBlock.nodeLineRenderType = "line"
dreamBlock.nodeLimits = { 0, 1 }
dreamBlock.depth = 8400
dreamBlock.placements = {
    name = "normal",
    data = {
        fastMoving = false,
        oneUse = false,
        width = 8,
        height = 8
    }
}

return dreamBlock
