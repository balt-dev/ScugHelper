local tile = ...
tile = tile or '3'

local cells
local cellWidth
local cellHeight

return {
    before = function(xo, yo, width, height)
        cells = {}
        math.randomseed(math.random(-16777216, 16777216), math.random(-16777216, 16777216))
        cellWidth = math.floor((width - 2) / 3)
        cellHeight = math.floor((height - 2) / 3)
        for y = 1, cellHeight do
            cells[y] = {}
            for x = 1, cellWidth do
                cells[y][x] = { left = true, top = true, visited = false }
            end
        end
        -- Mark first cell as visited
        cells[1][1].left = false
        cells[1][1].visited = true
        local cellStack = { { 1, 1 } }
        while #cellStack > 0 do
            -- Pop cell from stack
            local currentCell = cellStack[#cellStack]
            cellStack[#cellStack] = nil
            local x, y = table.unpack(currentCell)
            local neighbors = { "up", "left", "down", "right" }
            for i = 1, 4 do
                local rand = math.random(1, 4)
                neighbors[i], neighbors[rand] = neighbors[rand], neighbors[i]
            end
            while #neighbors > 0 do
                -- Check for any neighbors, and mark them if found
                if neighbors[#neighbors] == "up" and (y - 1 >= 1 and not cells[y - 1][x].visited) then
                    cells[y][x].top = false
                    cells[y - 1][x].visited = true
                    cellStack[#cellStack + 1] = { x, y }
                    cellStack[#cellStack + 1] = { x, y - 1 }
                    break
                elseif neighbors[#neighbors] == "down" and (y + 1 <= cellHeight and not cells[y + 1][x].visited) then
                    cells[y + 1][x].top = false
                    cells[y + 1][x].visited = true
                    cellStack[#cellStack + 1] = { x, y }
                    cellStack[#cellStack + 1] = { x, y + 1 }
                    break
                elseif neighbors[#neighbors] == "left" and (x - 1 >= 1 and not cells[y][x - 1].visited) then
                    cells[y][x].left = false
                    cells[y][x - 1].visited = true
                    cellStack[#cellStack + 1] = { x, y }
                    cellStack[#cellStack + 1] = { x - 1, y }
                    break
                elseif neighbors[#neighbors] == "right" and (x + 1 <= cellWidth and not cells[y][x + 1].visited) then
                    cells[y][x + 1].left = false
                    cells[y][x + 1].visited = true
                    cellStack[#cellStack + 1] = { x, y }
                    cellStack[#cellStack + 1] = { x + 1, y }
                    break
                else
                    neighbors[#neighbors] = nil
                end
            end
        end
    end,
    foreground = function(x, y, width, height)
        local absX = x + width / 2
        local absY = y + height / 2
        if absX == 0 or absY == 0 or absX == width - 1 or absY == height - 1 then return end
        absX = absX - 1
        absY = absY - 1
        local relX = absX % 3
        local relY = absY % 3
        if relX % 3 ~= 0 and relY % 3 ~= 0 then return end
        if (relX == 0 and relY == 0) then return tile end

        local cellX = math.floor(absX / 3) + 1
        local cellY = math.floor(absY / 3) + 1
        local cell = cells[cellY] and cells[cellY][cellX]
        if cell ~= nil then
            if relX == 0 and cell.left then return tile end
            if relY == 0 and cell.top then return tile end
        elseif cellY ~= cellHeight then return tile end
    end
}
