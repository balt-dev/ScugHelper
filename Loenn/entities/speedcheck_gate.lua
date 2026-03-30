local drawing = require("utils.drawing")

return {
    name = "ScugHelper/SpeedcheckGate",
    depth = 100,
    placements = {
        {
            name = "normal",
            data = {Angle = 0, Size = 32, Threshold = 240, Axis = 0, Comparison = 0, Action = 0, FlagName = "", SetX = 0, SetY = 0}
        },
    },
    draw = function(room, entity)
        drawing.callKeepOriginalColor(function()
            local lineX = math.cos(entity.Angle * math.pi / 180)
            local lineY = math.sin(entity.Angle * math.pi / 180)
            love.graphics.setColor { 0, 1, 1 }
            love.graphics.line(
                entity.x - lineX * entity.Size / 2,
                entity.y - lineY * entity.Size / 2,
                entity.x + lineX * entity.Size / 2,
                entity.y + lineY * entity.Size / 2
            );
        end)
    end,
    selection = function(room, entity)
        local lineX = math.cos(entity.Angle * math.pi / 180)
        local lineY = math.sin(entity.Angle * math.pi / 180)
        return utils.rectangle(
            entity.x - lineX * entity.Size / 2,
            entity.y - lineY * entity.Size / 2,
            math.max(8, lineX * entity.Size),
            math.max(8, lineY * entity.Size)
        )
    end,
    fieldInformation = {
        Axis = {
            type = "enum",
            options = { {"Horizontal", 0}, {"Vertical", 1}, {"Total", 2} }
        },
        Comparison = {
            type = "enum",
            options = { {"<", 0}, {"<=", 1}, {">", 2}, {">=", 3} }
        },
        Action = {
            type = "enum",
            options = { {"Kill", 0}, {"SetFlag", 1}, {"SetSpeed", 2} }
        }
    }
}
