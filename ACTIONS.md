# Actions

Actions are a pulse-based, level-global way of encoding highly custom logic in maps.

Actions may be placed anywhere within the map, including filler rooms, and will be alerted from anywhere.

Actions are not part of the ECS system, and calling `Remove()` on them is a no-op.

Each action has a list of groups it is in, and an execution delay.

There are a set of builtin action groups that alert when some ingame event happens, prefixed by `#`. These are autocompleted in Loenn.

For example, in order to kill the player after 3 seconds when they jump, you can do:

```
Action - Kill Player
Delay: 3.0
Groups: #PlayerJump, #PlayerWallJump, #PlayerSuperJump, #PlayerSuperWallJump, #PlayerClimbJump
```

Actions can also alert other actions. This is a simple loop that plays a sound every second:
```
Action - Forward
Delay: 1.0
Groups: SecondLoop, #InitActions
Targets: SecondLoop

Action - Play Sound
Delay: 0.0
Groups: SecondLoop
Path: event:/game/general/thing_booped
```

Additionally, there are Action Gates and Action Triggers that alert set action groups when crossed.

Action Gates, Triggers, and Actions themselves cannot alert builtin action groups, and will error on level load if configured to.

Additionally, there is the Global Trigger Flag Listener, used to convert any trigger into something usable globally by Actions or anything else that can use flags.

The Global Trigger Flag Listener, when placed atop a Trigger, converts that trigger into a level-global controller. The trigger will call `OnEnter` when the configured flag transitions from `false` to `true`, will call `OnStay` every frame the flag is `true`, and will call `OnLeave` when the flag transitions from `true` to `false`.

Like actions, triggers configured to be level-global controllers are not within the ECS system, and are not reset between respawns. Calling `Remove()` on a trigger made to be level-global is a no-op.
