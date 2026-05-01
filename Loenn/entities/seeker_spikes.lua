local spikeHelper = require("helpers.spikes")

local spikeOptions = {
    directionNames = {
        up = "ScugHelper/SeekerSpikes-Up",
        down = "ScugHelper/SeekerSpikes-Down",
        left = "ScugHelper/SeekerSpikes-Left",
        right = "ScugHelper/SeekerSpikes-Right"
    }
}

return spikeHelper.createEntityHandlers(spikeOptions)