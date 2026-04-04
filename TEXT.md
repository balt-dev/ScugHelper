# Text Entities

Text entities display a specific string in the map.

Any text in a text entity's input outside of {curly braces} is left raw,
and curly braces specify a specifc formatter for the text inside them.

Formatters have a format of `{name:value}`. Not specifying a name makes it default to the empty string.

Here is a list of all formatters:

- `{LOC_KEY}` or `{:LOC_KEY}` - The string specified by the key in the current localization file. Should ideally be used whenever possible.
- `{flag:FlagName}` - Either `true` or `false`, depending on whether the given flag is set.
- `{counter:CounterName}` - The integer value of the given counter, or 0 if it doesn't exist.
- `{slider:SliderName}` - The value of the given slider, formatted as a percentage.
- `{playerX:}` - The player's current X value, as an integer.
- `{playerY:}` - The player's current Y value, as an integer.
- `{playerSubpixelX:}` - The player's current subpixel X value, with 3 decimal places of precision.
- `{playerSubpixelY:}` - The player's current subpixel Y value, with 3 decimal places of precision.
- `{playerSpeed:}` - The player's current total speed, as an integer.
- `{playerSpeedX:}` - The player's current X speed, as an integer.
- `{playerSpeedY:}` - The player's current Y speed, as an integer.
- `{playerStamina:}` - The player's current stamina, as an integer.
- `{playerDashes:}` - The player's current dash count, as an integer.
- `{playerName:}` - The player's save file name.
- `{dashCount:}` - The player's total dash count in the level.
- `{deathCount:}` - The player's total death count in the level.
- `{deathRoomCount:}` - The player's total death count in the current room.
- `{time:}` - The current session time.
