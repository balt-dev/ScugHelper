local scughelper = require "scughelper"

local seed = 0

return {
    before = function()
        -- runs before all foreground and background calls
        seed = math.random(-16777216, 16777216)
    end,
    foreground = function(x, y, width, height)
        local perlinVal = scughelper.perlin(y / 32, x / 16, 5, 0.5, seed)
        local distance = (perlinVal - (((y + 48) / 105))) * 105
        -- return tileset index
        if distance <= -1 + math.random() - 0.5 then return '1' end
        if distance <= 0 then return '3' end
        -- nil leaves it empty
    end,
    background = function(x, y, width, height)
        local perlinVal = scughelper.perlin(y / 32, x / 16, 5, 0.5, seed)
        local distance = (perlinVal - (((y + 48) / 105))) * 105
        if distance <= 10 + (math.random() - 0.5) * 3 then return '3' end
    end
}
