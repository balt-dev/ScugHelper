return {
    name = "ScugHelper/SessionVariableTrigger",
    depth = -1e20,
    placements = {
        {
            name = "flag",
            data = { width = 32, height = 32, Name = "DummyFlag", Type = 0, Value = "true", CoverRoom = false, ResetOnLeave = false }
        },
        {
            name = "counter",
            data = { width = 32, height = 32, Name = "DummyCounter", Type = 1, Value = "1", CoverRoom = false, ResetOnLeave = false }
        },
        {
            name = "slider",
            data = { width = 32, height = 32, Name = "DummySlider", Type = 2, Value = "1.0", CoverRoom = false, ResetOnLeave = false }
        },
    },
    fieldInformation = function(trigger)
        return {
            Type = {
                options = { { "Flag", 0 }, { "Counter", 1 }, { "Slider", 2 } },
                editable = false
            },
            Value = {
                validator = function(value)
                    if (trigger.Type == 0) then
                        return (value == "true" or value == "false")
                    elseif (trigger.Type == 1) then
                        return value:find("%-?%d+") ~= nil
                    else
                        return tonumber(value) ~= nil
                    end
                end
            }
        }
    end,
    triggerText = function(room, trigger)
        if (trigger.Type == 0) then
            return "Set Flag"
        elseif (trigger.Type == 1) then
            return "Set Counter"
        else
            return "Set Slider"
        end
    end,
    fieldOrder = {"x", "y", "width", "height", "Name", "Value"}
}
