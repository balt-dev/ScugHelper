local sliderName, sliderA, sliderB = ...
sliderA = tonumber(sliderA)
sliderB = tonumber(sliderB)

local function updateSlider(x, y, width, height)
    local playerX = scughelper.sliders["ScugHelper.PlayerX"] + scughelper.sliders["ScugHelper.PlayerSubpixelX"]
    local relativeX = playerX - x
    local strength = relativeX / width
    strength = math.min(math.max(strength, 0), 1)
    local value = sliderA + (sliderB - sliderA) * strength
    scughelper.sliders[sliderName] = value
end

return {
    onEnter = updateSlider,
    onStay = updateSlider,
    onLeave = updateSlider,
}
