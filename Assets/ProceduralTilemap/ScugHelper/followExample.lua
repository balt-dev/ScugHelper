local scughelper = require "scughelper"

return {
    foreground = function(x, y, width, height)
        local perlinVal = scughelper.perlin(y / 32, x / 32, 5, 0.5)
        local distance = (perlinVal - (((y + 48) / 125))) * 125
        -- return tileset index
        if distance <= -2  then return '1' end
        if distance <= -0.01 then return '3' end
        -- nil leaves it empty
    end,
    background = function(x, y, width, height)
        local perlinVal = scughelper.perlin(y / 32, x / 32, 5, 0.5)
        local distance = (perlinVal - (((y + 48) / 125))) * 125
        if distance <= 10 then return '3' end
    end
}
