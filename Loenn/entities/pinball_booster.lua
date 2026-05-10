return {
    name = "ScugHelper/PinballBooster",
    depth = -100,
    placements = {
        {
            name = "blue",
            data = { red = false, accelX = 0, accelY = 0, limitX = 500, limitY = 500, speed = 240, Bounce = true }
        },
        {
            name = "red",
            data = { red = true, accelX = 0, accelY = 0, limitX = 500, limitY = 500, speed = 240, Bounce = true }
        }
    },
    texture = function(room, entity)
        if (entity.Bounce == nil) then entity.Bounce = true end
        return (entity.Bounce and ("objects/ScugHelper/pinballBooster/" .. ((entity.red and "boosterRed00") or "booster00")))
            or ("objects/booster/" .. ((entity.red and "boosterRed00") or "booster00"))
    end,
    fieldInformation = {
        AttachedEntityID = {
            fieldType = "integer"
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
