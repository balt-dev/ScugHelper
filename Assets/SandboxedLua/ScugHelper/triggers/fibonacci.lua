local scughelper = require "scughelper"

local first = 0.0
local second = 1.0

local index = 1

scughelper.sliders["fibonacciNumber"] = second
scughelper.counters["fibonacciIndex"] = index

return {
    onEnter = function()
        index = index + 1
        local sum = first + second
        first = second
        second = sum
        scughelper.sliders["fibonacciNumber"] = second
        scughelper.counters["fibonacciIndex"] = index
    end
}
