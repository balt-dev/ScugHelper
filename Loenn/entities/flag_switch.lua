local flagToggle = {}

flagToggle.name = "ScugHelper/FlagSwitch"
flagToggle.depth = 2000

function flagToggle.texture(room, entity)
    if entity.AllowRevert then
        return entity.SpritePath .. "/switch01"
    elseif entity.State then
        return entity.SpritePath .. "/switch15"
    else
        return entity.SpritePath .. "/switch13"
    end
end

flagToggle.placements = {
    {
        name = "both",
        data = {
            AllowRevert = true,
            Flag = "ScugHelper.RetryDisabled",
            State = true,
            SpritePath = "objects/coreFlipSwitch"
        },
    },
    {
        name = "on",
        data = {
            AllowRevert = false,
            Flag = "ScugHelper.RetryDisabled",
            State = true,
            SpritePath = "objects/coreFlipSwitch"
        },
    },
    {
        name = "off",
        data = {
            AllowRevert = false,
            Flag = "ScugHelper.RetryDisabled",
            State = false,
            SpritePath = "objects/coreFlipSwitch"
        },
    }
}

return flagToggle
