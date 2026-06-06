return {
    name = "ScugHelper/PinballBooster",
    depth = -100,
    placements = {
        {
            name = "blue",
            data = { red = false, accelX = 0, accelY = 0, limitX = 500, limitY = 500, speed = 240, SquareHitbox = false, HitDashColliders = false, BounceLimit = -1 }
        },
        {
            name = "red",
            data = { red = true, accelX = 0, accelY = 0, limitX = 500, limitY = 500, speed = 240, SquareHitbox = false, HitDashColliders = false, BounceLimit = -1 }
        }
    },
    texture = function(room, entity)
        return ((entity.Bounce ~= false and entity.BounceLimit ~= 0) and ("objects/ScugHelper/pinballBooster/" .. ((entity.red and "boosterRed00") or "booster00")))
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
