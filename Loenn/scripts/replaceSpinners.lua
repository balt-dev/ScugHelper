local script = {
    name = "replaceSpinners",
    displayName = "Replace Vanilla Spinners",
    tooltip = "Replaces all vanilla spinners with ScugHelper GPU spinners.",
}

local function replaceSpinner(entity)
    entity._name = "ScugHelper/GPUSpinner"
    entity.SpritePath = "danger/crystal"
    entity.Rainbow = false
    entity.dust = nil
    entity.Color = "ffffff"
    entity.color = entity.color:lower()
    if entity.color == "red" then
        entity.SpriteSuffix = "_red"
        entity.ShatterColor = "ff4f4f"
    elseif entity.color == "purple" then
        entity.SpriteSuffix = "_purple"
        entity.ShatterColor = "ff4fef"
    elseif entity.color == "blue" then
        entity.SpriteSuffix = "_blue"
        entity.ShatterColor = "639bff"
    elseif entity.color == "rainbow" then
        entity.Rainbow = true
        entity.SpriteSuffix = "_white"
        entity.ShatterColor = "ffffff"
    else
        entity.SpriteSuffix = "_white"
        entity.ShatterColor = "ffffff"
    end
    entity.color = nil
end

function script.run(room, args)
    for _, ent in ipairs(room.entities) do
        if ent._name == "spinner" and not ent.dust and ent.color ~= "core" then
            print("Replacing spinner with ID " .. ent._id)
            replaceSpinner(ent)
        end
    end
end

return script
