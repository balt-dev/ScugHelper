return {
    name = "ScugHelper/PinballBooster",
    depth = -100,
    placements = {
        {
            name = "blue",
            data = {red = false, accelX = 0, accelY = 0, limitX = 500, limitY = 500}
        },
        {
            name = "red",
            data = {red = true, accelX = 0, accelY = 0, limitX = 500, limitY = 500}
        }
    },
    texture = function(room, entity)
        return "objects/pinballBooster/" .. ((entity.red and "boosterRed00") or "booster00")
    end,
    fieldInformation = {
        AttachedEntityID = {
            fieldType = "integer"
        },
        speed = {
            fieldType = "number",
        },
        accelX = {
            fieldType = "number",
        },
        accelY = {
            fieldType = "number",
        },
        limitX = {
            fieldType = "number",
            minimumValue = 0
        },
        limitY = {
            fieldType = "number",
            minimumValue = 0
        }
    }
}
