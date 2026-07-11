## (0.1.1)
- **Addition**: Add testing maps

## (0.1.2)
- **Addition**: Add randomizer support to the test maps because why the hell not

## (0.2.0)
- **Addition**: Speedcheck Gate

## (0.3.0)
- **Addition**: Holdable Gates

## (0.3.1)
- **Bugfix**: Player Seekers can now interact with Dash Switches

## (0.3.2)
- **Adjustment**: Make player seeker fix toggleable

## (0.4.0)
- **Addition**: Text entities

## (0.5.0)
- **Addition**: Square Crystal
- **Addition**: Recoil Bumper
- **Addition**: Dashless Crystal Heart
- **Improvement**: Added Depth field to Text
- **Removal**: Removed rando stuff for testing map bc it was too annoying to keep updated

## (0.5.1)
- **Bugfix**: Hopefully fix weird crash with rebound blocks

## (0.6.0)
- **Overhaul**: Improved appearance of Booster Fields
- **Addition**: Acceleration Fields
- **Overhaul**: Added Gravity toggle to Square Gems

## (0.6.1)
- **Improvement**: Make Holdable Gates look better when used

## (0.6.2)
- **Tweak**: Change how Acceleration Field drag works
- **Improvement**: Add controller option to Acceleration Fields
- **Improvement**: Tweak how square crystals bounce

## (0.6.3)
- **Bugfix**: Fix text in Lönn

## (0.6.4)
- **Bugfix**: Fix various entities in Lönn

## (0.7.0)
- **Addition**: Grounded Refills
- **Addition**: Hiccup Refills

## (0.7.1)
- **Tweak**: Remove some unused entries in the Lönn localization file

## (0.8.0)
- **Addition**: Midair Refills

## (0.8.1)
- **Improvement**: Code cleanup

## (1.0.0)
- **Addition**: Flag-Controlled Temple Eye
- **Addition**: Fastfall Block
- **Addition**: Freeze Refill
- **Addition**: Speed Refill
- **Addition**: Tungsten Cube
- **Improvement**: Holdable Gates now check the top of the player as well
- **Optimization**: Improve performance of Booster Fields
- **Improvement**: Refactored Hiccup Refills to allow code reuse between all custom refills
- **Bugfix**: Midair Refills are now only consumed when used and are properly removed when level is reloaded
- **Improvement**: Add crush effects to Recoil Bumpers and Square Crystals

## (1.0.1)
- **Bugfix**: Update Pinball Boosters squish effect on Update instead of Render
- **Improvement**: Give Tungsten Cubes a terminal velocity of 1000 px/sec

## (1.0.2)
- **Addition**: Added a few debug commands, for fun

## (1.1.0)
- **Addition**: Refill Gates

## (1.2.0)
- **Overhaul**: Added Lönn descriptions for all entities and triggers
- **Addition**: Level End Controller

## (1.2.1)
- **Bugfix**: Fix Level End controller triggering a million times

## (1.3.0)
- **Addition**: Acceleration Field
- **Addition**: trackspeed Debug Command

## (1.3.1)
- **Tweak**: Make Tungsten Cubes break Crystal Hearts

## (1.3.2)
- **Addition**: Made Tungsten Cubes affect your max fall speed while held

## (1.3.3)
- **Improvement**: Gripwalls can now attach to moving solids
- **Tweak**: Flag-Controlled Temple Eyes now subclass Temple Eyes internally

## (1.3.4)
- **Bugfix**: Cyclers now properly work with several more vanilla entities

## (2.0.1)
- **Addition**: Action System with several Action entities
- **Overhaul**: Separated entities into proper namespaces under the hood
- **Addition**: Outline entity
- **Addition**: Teleport Trigger
- **Improvement**: FrostHelper integration for Text entities
- **Tweak**: Increased threshold for Tungsten Cube crush
- **Optimization**: Cyclers and gates no longer draw every frame in Lönn

Actions are a major update to the mod. They allow level-global custom logic without the overhead of having to keep a global room loaded. (See: bits & bolts. No shade though, that mod is awesome.)
Actions are never actually placed into a room when the level is loaded, and are instead stored in the ActionManager static class.
The paradigm for Actions is inspired by Geometry Dash triggers and DiamondFire plot events.

## (2.1.0)
- **Addition**: Custom Entity Field
- **Optimization**: Outline entities now prebake their textures

## (2.2.0)
- **Addition**: Controllable Falling Blocks
- **Addition**: Neutralless Ice Walls/Wall Boosters

## (2.3.0)
- **Tweak**: Adjusted camera offset while holding Tungsten Cubes
- **Bugfix**: Right-facing neutralless ice walls now attach properly

## (2.3.1)
- **Tweak**: Increased gravity of tungsten cubes slightly
- **Bugfix**: Temple Fall and Reflection Fall states now break Fastfall blocks
- **Bugfix**: Fix Action - Set Player State checking the wrong field

## (2.4.0)
- **Addition**: Flag Gate (sets flag when crossed)
- **Improvement**: Text Entities can now be flag-toggleable

## (2.5.0)
- **Improvement**: Fastfall Blocks now have a configurable minimum speed
- **Addition**: Action - Snowball

## (2.6.0)
- **Addition**: Sideflipping
- **Bugfix**: Vanilla Wall Boosters no longer have a broken texture
- **Improvement**: Teleport triggers now work across rooms

## (2.7.0)
- **Addition**: Action Dash Switch
- **Addition**: Action Temple Gate
- **Addition**: Fragile Seeker
- **Addition**: Limbo Refill
- **Addition**: Limbo Field
- **Addition**: Overcharge Refill
- **Addition**: Seeker Temple Gate
- **Addition**: Starjump Railing
- **Addition**: Starjump Tileset
- **Addition**: Spinners Are Seekers config option
- **Improvement**: Sideflips now are affected by holding a Tungsten Cube
- **Tweak**: Tungsten Cubes now reduce walljump height more
- **Bugfix**: Teleport Triggers/Gates now teleport the camera properly

## (2.7.1)
- **Bugfix**: Fix mod incompatibility with Overcharge refills

## (2.7.2)
- **Bugfix**: Fix mod incompatibility with Fragile Seekers

## (2.8.0)
- **Addition**: Subpixel Kill Field
- **Addition**: Seeker Refill
- **Addition**: Seeker Trigger
- **Addition**: De-Seeker Field
- **Addition**: Set Refill Field
- **Addition**: Builtin Session Variables
- **Addition**: Global Trigger Action Listener
- **Addition**: Global Entity Action Listener
- **Addition**: Entity Globalizer
- **Improvement**: Pinball Boosters now can customize their speed

## (2.8.2)
- **Addition**: Seeker Spinners
- **Addition**: Seeker Spikes
- **Bugfix**: Subpixel Kill Field now works properly when intersected by more than 1 pixel

## (2.9.0)
- **Addition**: Save Touch Switches Trigger

## (2.9.1)
- **Addition**: Moving Seeker Barrier
- **Addition**: Rotating Seeker Spinner
- **Addition**: Track Seeker Spinner

## (2.10.0)
- **Addition**: Debug Minimap

The minimap is off by default, but can be toggled on in the mod settings.

## (2.11.0)
- **Addition**: Static Moon Block
- **Addition**: Angle Bumper

## (2.12.0)
- **Addition**: Hang Rails

## (2.12.2)
- **Improvement**: Hang Rails are much more customizable
- **Bugfix**: Limbo Refills no longer break with certain mods enabled

FUCK THAT BUG OH MY GODDDDDD

## (2.13.0)
- **Addition**: Properly add the Collide Gate
- **Improvement**: Alter the Action Listener Controller to also work with triggers
- **Overhaul**: Create a wiki

## (2.14.3)
- **Addition**: Custom Swap Block

## (2.15.0)
- **Improvement**: Adjusted Gripwall colors to be more colorblind-friendly
- **Addition**: Count Entities Trigger
- **Overhaul**: Certian Special Session Variables can now be assigned to
- **Addition**: Action - Lightning Strike
- **Addition**: Flag Clutter Switch
- **Addition**: Flag Clutter Gate
- **Addition**: Tightropes
- **Addition**: Conveyor Belts
- **Addition**: Editor Hide Controller
- **Addition**: Debug Wall
- **Addition**: Camera Blocker
- **Improvement**: Updated Custom Entity Fields to use SIDs - legacy ones are unaffected
- **Improvement**: Moved object sprites to a directory
- **Addition**: Death Effect Trigger

## (2.15.1)
- **Improvement**: Made Camera Blockers less jank
- **Addition**: Camera Blockers now have a Priority system
- **Bugfix**: Fixed mod incompatibility with Overcharge Refills

## (2.15.2)
- **Overhaul**: GravityHelper support

## (2.15.3)
- **Addition**: Refill Rects

## (2.16.0)
- **Addition**: Jellyfish Fizzle Gate
- **Addition**: Center Camera Trigger

Going to have to start deleting the oldest 2.x versions from now on, as GameBanana has a file limit. I'm keeping 2.0, though.

## (2.16.1)
- **Improvement**: Made Pinball Boosters a bit more customizable

## (2.17.0)
- **Addition**: Macabre Booster

## (2.17.2)
- **Bugfix**: Fix Brass Berry Blocks referencing wrong image path

## (2.17.3)
- **Improvement**: Refill rects now fallback to a predefined refill if they don't have one placed on them

## (2.17.8)
- **Bugfix**: Fix accidental regression with special session variables

## (2.18.0)
- **Addition**: Action Cassette Blocks

## (2.18.1)
- **Tweak**: Tweaked when Action Cassette Blocks play their switch sound

## (2.19.0)
- **Addition**: ScugHelper.SaveQuitDisabled

## (2.19.1)
- **Improvement**: Manual Cassette Blocks can now be driven by a Flag

## (2.19.3)
- **Bugfix**: Fix Flag Manual Cassette Blocks not respecting the set Flag State

## (2.20.0)
- **Addition**: Cassette Flag Controller

## (2.20.1)
- **Improvement**: Allow Cassette Flag Controllers to have a 1-length span

## (2.20.2)
- **Improvement**: Manual/Flag Cassette Blocks can now be Silent

## (2.21.0)
- **Addition**: Outline Spinners
- **Addition**: Outline Spinner Color Controller

## (2.21.1)
- **Addition**: Add Bloom checkbox to outline spinner color controller

## (2.21.2)
- **Addition**: Added some more demonstration rooms to the Actions showcase

## (2.22.0)
- **Addition**: Dash Restriction Trigger
- **Addition**: Custom Kevin Controller
- **Improvement**: Rebound blocks are less buggy

## (2.23.0)
- **Tweak**: Rename Booster Field to Booster Barrier
- **Tweak**: Rename Entity Field to Entity Barrier
- **Tweak**: Rename Limbo Field to Limbo Barrier
- **Addition**: Flag Barrier
- **Improvement**: Cassette Flag Controller now works with two spans on the same flag
- **Improvement**: Lönn now shows builtin flags in the Flag entry

## (2.23.1)
- **Tweak**: Move the showcase map around a bit

## (2.23.2)
- **Addition**: Always Overcharge setting
- **Improvement**: Refill Fields can now cover the room

## (2.24.0)
- **Addition**: Mid-Step Portals

## (2.24.1)
- **Improvement**: Allow Mid-Step Portals to be vertically offset

## (2.24.2)
- **Bugfix**: Fix ScugHelper.PlayerY and ScugHelper.PlayerSpeedY accidentally returning the X values instead

## (2.24.3)
- **Bugfix**: Fix debug rendering for Mid-Step Portals
- **Bugfix**: Fix alignment issues with Mid-Step Portals
- **Tweak**: Moved Mid-Step Portals above SolidTiles

## (2.24.4)
- **Tweak**: Adjust Overcharge Refill slightly

## (2.24.5)
- **Tweak**: Adjusted how Overcharge Refills react to wallbouncing

## (2.24.6)
- **Tweak**: Change how Mid-Step Portals are placed
- **Addition**: Vertical Mid-Step Portals
- **Tweak**: Tweak Overcharge Refills again

## (2.24.7)
- **Tweak**: Make Overcharge Refills always remove screen transition dash cooldown

## (2.24.8)
- **Tweak**: Make some of the sillier Overcharge Refill behavior behind a flag

## (2.25.0)
- **Addition**: Remove Tiles Controller
- **Addition**: Flag Invisible Barrier

## (2.25.1)
- **Bugfix**: Fix showcase map breaking without FrostHelper

## (2.25.2)
- **Overhaul**: Move the showcase maps into the mod settings

This one goes out to the like fifty people at my throat in the Celeste discord for having showcase maps in my helper

## (2.26.0)
- **Addition**: Limbo Glitch State Trigger
- **Addition**: Exit Limbo Glitch State Trigger
- **Improvement**: Teleport triggers can now have a delay and/or always reload the room

Go play Limbo by Somera :)

## (2.26.1)
- **Bugfix**: Fix inaccuracy with Limbo Glitch State Trigger not setting Player.Collidable = false
- **Improvement**: Limbo Glitch State Trigger now has a toggle to lock the player's state machine

## (2.27.0)
- **Refactor**: Deprecated old angle bumpers
- **Overhaul**: Overhauled Angle Bumpers

Old angle bumpers will stay in maps, but can no longer be placed in Lönn. Migrate if possible.

## (2.27.3)
- **Bugfix**: Fix angle bumpers with nodes on their center breaking Lönn

## (2.28.0)
- **Addition**: Procedurally Generated Tilemap
- **Bugfix**: Pinball boosters can no longer get you stuck in the ground when exiting square hitbox
- **Improvement**: Limbo glitch state trigger no longer forces the player to StNormal
- **Addition**: `ScugHelper.EpochTime` Special Counter

Please read the wiki before using the procedural tilemaps.

## (2.28.1)
- **Bugfix**: Fixed Player Seeker moving with minimap focused
- **Improvement**: Player Seekers now respect Dash Angle Restriction Triggers

## (2.28.2)
- **Tweak**: Teleport triggers no longer kill the player if target is placed outside of a room
- **Addition**: Minimap Hide keybind

## (2.28.3)
- **Optimization**: Made StarjumpOutlineRenderer and SeekerBarrierMaskRenderer not update or render if no entities are on screen that use them
- **Addition**: Instant Room Transitions mod setting

## (2.29.0)
- **Addition**: Input Spam Trigger
- **Addition**: Proximity Temple Eye
- **Addition**: Elsewhere Watchtower

## (2.29.1)
- **Addition**: Unstuck Trigger

## (2.29.2)
- **Tweak**: Reworked Spam Input trigger
- **Tweak**: Reworked Subpixel Killboxes

## (2.29.3)
- **Bugfix**: Fix subpixel killboxes

100 updates :tada:

## (2.30.0)
- **Addition**: Sandboxed Lua Trigger
- **Addition**: Sandboxed Lua Gate
- **Addition**: Action - Sandboxed Lua
- **Improvement**: Lua now has a runtime limit
- **Improvement**: Entities using Lua files now rerun the Lua file's chunk when loaded
- **Addition**: New entity: Kickpad

This is the 100th update of ScugHelper, and with it I bring the power of Lua! It's sandboxed and limited (unlike LuaCutscenes >.>) to prevent bad actors from stealing your credit card info or something LMAO
The Lua action, trigger, and gate are equivalent with the FrostHelper Session Expression Action suite in what they can do, but are generally more concise for complex logic - however, Lua is a programming language, and with it comes the woes of writing code. Session Expression actions are still there if you don't want to write any Lua.

## (2.30.1)
- **Improvement**: Added memory allocation limit to Lua scripts

## (2.31.0)
- **Bugfix**: Fix potential overflow when setting Lua memory allocation limit too high
- **Addition**: Extended Variant Mode interop with Special Session Variables

## (2.32.0)
- **Overhaul**: Rework Acceleration Fields
- **Addition**: Instant Flag Block

## (2.32.1)
- **Optimization**: Optimize the performance of Special Session Variables
- **Addition**: Added a few more Special Session Variables

## (2.32.2)
- **Bugfix**: Fix Instant Room Transitions adding way more time than they should

## Making Things Right (2.33.0)
- **Tweak**: Special Session Variables no longer save to the Session without an Eager Special Session Variable Controller in the room
- **Overhaul**: Text entities now have a font set by the mapper

## Text Entity Hotfix and Additions (2.33.1)
- **Optimization**: Text Entities now prebake their texture on change to prevent renderer churn
- **Addition**: More builtin Text Entity fonts are now available in Lönn

## Hotfix - Logging and Text Bounds (2.33.2)
- **Bugfix**: Fix text entities clipping off the very bottom pixel of their outline
- **Bugfix**: Fix extraneous logging in release builds

## Disabling with Flags & Dash Snap (2.34.0)
- **Addition**: Snap To Consistent Dash Position Trigger
- **Addition**: ScugHelper.PauseDisabled SSV
- **Addition**: ScugHelper.SaveQuitDisabled SSV
- **Addition**: ScugHelper.RetryDisabled SSV

## Quicker Following Tilemaps (2.34.5)
- **Optimization**: Procedural tilemaps set to follow the player now only regenerate what they need to

## Hotfix - Hitbox crashfix (2.34.6)
- **Bugfix**: Fix static procedural tilemaps crashing the game when hitboxes are displayed

## CommunalHelper (2.35.0)
- **Addition**: CommunalHelper support

## Hotfix - Optimize Acceleration Fields (2.35.1)
- **Optimization**: Heavily optimize acceleration fields

## Hotfix - Strange crash with Acceleration Fields (2.35.2)
- **Bugfix**: Fix strange crash that happens sometimes with Accleration Fields somehow not being tracked

## Procedural Tilemaps in Minimap (2.35.3)
- **Improvement**: Procedural Tilemaps now render in the Minimap if you're in the same room as them

## Grab Bag, Two! (2.36.0)
- **Addition**: Showcase Map 2 (Electric Boogaloo)
- **Addition**: Refill Crystals
- **Addition**: Arbitrary Angle Spring
- **Addition**: Holdable Trajectory Controller
- **Addition**: Boost Refill

## Hotfix - ModInterop.cs (2.36.1)
- **Addition**: Added a ModInterop API for other mods
- **Optimization**: The Debug Minimap no longer renders at all if opacity is below 0.01
- **Bugfix**: Angle Bumpers no longer fuck up the player's position
- **Bugfix**: Track/Rotate Seeker Spinners no longer kill the player
- **Optimization**: Seeker Spinners/Spikes are slightly less expensive to render

## They Do Not Call It A Fucking Jraphics Card (2.37.0)
- **Optimization**: Sped up Seeker Spinners/Spikes massively by moving their effect to the GPU
- **Optimization**: Moved the particle effects of Refill Fields, De-Seeker Fields, Flag Barriers, Booster Barriers, and Limbo Barriers to the GPU
- **Addition**: GPU Spinners
- **Bugfix**: Pinball Boosters no longer stick to the ground when hitting at shallow angles
- **Improvement**: Refill Crystals now properly play their refill sound when shattered

## Hotfix - Waiting for Godot (2.37.1)
- **Improvement**: Added support for MotionSmoothing to prevent jitter on procedural tilemaps

## Hotfix - Culling go BRRRRR (2.37.3)
- **Optimization**: Wobbly barriers now do camera culling for performance
- **Improvement**: Dream blocks no longer reset speed with Overcharge refills
- **Bugfix**: Refill Crystals can now be activated with a Crouch Dash
- **Improvement**: Instant hypers now work with overcharge refills
- **Improvement**: Refill Crystals now bounce on springs
- **Addition**: DeterministicAutotiling setting
- **Improvement**: Brass Berry and Sideflipping now use proper namespaced flags
- **Improvement**: Major internal code cleanup

## Let's Have A Watch (2.38.0)
- **Addition**: Seekable Playback Watchtower

## Hotfix - Let's Fix A Bug (2.38.1)
- **Bugfix**: Seekable Playback Watchtowers no longer teleport to their start position on repeated level loads
- **Bugfix**: Seekable Playback Watchtowers now clear the player hair cache on level exit/reload

I can never just have a .0 without having to immediately fix something, huh.

## Patch - Get Jiggy With It (2.38.2)
- **Tweak**: Tweaked how the EnableSillyOverchargeBehavior flag functions to allow for broader compatibility

## Patch - Oops, I Broke Something (2.38.3)
- **Addition**: Added ScugHelper.LegacyOvercharge flag to use legacy silly overcharge behavior

## Hotfix - r/im14andthisisdeep (2.38.4)
- **Tweak**: Set depth of playback in seekable playback watchtowers to 1
- **Tweak**: Made seekable playback watchtowers default to a seek speed of 0.7

## Patch - Odds & Ends (2.38.5)
- **Bugfix**: Moved refill crystal light to prevent it accidentally getting occluded
- **Tweak**: Made Seekable Playback Watchtowers default to not having set position - nodeless ones will use the nearest spawnpoint's position

## Across The Levelverse (2.39.0)
- **Addition**: Level Teleport Trigger

## Hotfix - Stop! Hammer Time (2.39.2)
- **Bugfix**: Fix Limbo refills preventing hitbox changes on collision
- **Addition**: Static Hitbox Trigger because the above behavior was too cool to not immortalize somehow
- **Bugfix**: Fix every level sending you to Prologue when you exit it

## Patch - GWA (2.39.3)
- **Tweak**: Made the #PlayerGrab builtin action group trigger on grab press even when not next to a wall for parity with other mods that do similar

## Adding More to the Grab Bag (2.40.0)
- **Addition**: Physical Sand
- **Addition**: Dream Field
- **Addition**: Raycasted Audio Controller
- **Bugfix**: Action Swap Blocks no longer give stale liftboost when not moving
- **Improvement**: Hangrails now work with the MotionSmoothing mod
- **Bugfix**: Hangrails no longer freak out when grabbed at the edge of the rail
- **Improvement**: Hangrails now give liftboost when dropped off of the edge
- **Tweak**: The Minimap is now hidden by default on game launch
- **Improvement**: Angle Bumpers now work with the DisableSuperBoosts and ExplodeLaunchSpeed extended variants
- **Addition**: Limbo/Entity/Flag/Booster Barriers can now have a custom Depth
- **Addition**: Boost Refills now have a Cancel Dash option

## Patch - Dreamsmuggle Shenanigans (2.40.1)
- **Improvement**: Holdables can now be grabbed while dreamdashing inside of a Dream Field
- **Improvement**: Refill Crystals now work when dashing from inside a Dream Field

## Patch - Upgrades, People! (2.40.2)
- **Improvement**: Refill Crystals have been upgraded to Player Collider Crystals and now work with non-refills
- **Improvement**: Text entities can now have a custom opacity and parallax
- **Improvement**: The Level Teleport Trigger now works even if the current level isn't fully initialized yet
- **Bugfix**: GPU Spinners now properly hide when Visible is set to false
- **Bugfix**: The Dream Field activate/deactivate animation now works properly when played more than once

## My Head Hurts (2.41.0)
- **Addition**: Hall of Mirrors Styleground
- **Improvement**: Can now throw from inside a Dream Field
- **Bugfix**: Demodashing out of a Dream Field now works properly

## Hotfix - Save States Break Everything (2.41.1)
- **Bugfix**: Hopefully fixed Hall of Mirrors with savestates

## Patch - Madeline They Put Me In A Cube (2.41.2)
- **Addition**: Tungsten Cubes can now be retextured
- **Addition**: Tungsten Cubes now have the KillOnDestroy, NoLeaveBehind, and CrystalSounds fields

## Patch - GPU go BRR (2.41.3)
- **Optimization**: GPU Spinners are now a lot faster
- **Improvement**: All refills now have properly colored particles

## Patch - They Raised a White Flag (2.41.4)
- **Addition**: Properly add the Flag Refill that was meant to be added last update

## Hotfix - Culling Blunder (2.41.5)
- **Bugfix**: Fix GPU spinners not unculling themself after coming back onscreen after going offscreen

## why do they call it oven when you of in the cold f (2.42.0)
- **Addition**: Baked Spinners Controller
- **Addition**: Mod setting - Always Bake Spinners

## Patch - GPU Go BRRRR (again) (2.42.1)
- **Addition**: Readded the Replace Vanilla Spinners mod option alongside the Always Bake Spinners mod option
- **Improvement**: GPU spinners can now properly shatter

## Patch - Some Spring Cleaning Never Hurt Anyone (2.42.2)
- **Addition**: Can now optionally show the showcase maps in map select (hidden by default)
- **Improvement**: The Holdable Trajectory Controller now accepts a multiplier for inherited speed
- **Bugfix**: Fixed a crash with GPU spinners and vanilla spinners placed next to each other
- **Bugfix**: Flag refills should hopefully not crash the game anymore under circumstances when the level isn't fully initialized yet
- **Improvement**: Refill Rectangles now have a customizable respawn timer

## The Funny (2.43.0)
- **Addition**: Fuck Block (yes it's actually called that)
- **Addition**: Jump Through Block

## 2x Release Combo!! (2.44.0)
- **Addition**: Balatro Styleground
- **Addition**: Unforgiving Lava/Ice

## Wibbly Wobbly Timey Wimey (2.45.0)
- **Addition**: Custom Distort
- **Addition**: Instant Transition Controller
- **Improvement**: Instant Transitions now skip the darkness fadee and player NoControl frames

## Pumber Line Decals My Beloathed (2.46.1)
- **Addition**: Polyline
- **Improvement**: Major code cleanup under the hood

## Hotfix - FUCJ (2.46.3)
- **Bugfix**: Fix game crash on startup

## Patch - «mmm soup» (2.46.4)
- **Improvement**: Add a few new fields to Polylines

## rain worl (2.47.0)
- **Addition**: Spear

## Patch - Okay that's a bit too broken actually (2.47.1)
- **Tweak**: Made infinite spears only replenish when dash does

## Patch - It's NERF or Nothing (2.47.2)
- **Addition**: ScugHelper.InfiniteSpearCooldown slider

## Patch - haha spearflip go brr (2.47.3)
- **Addition**: Added 'spear' debug command to give a spear on command

## Hotfix - Bonk! (2.47.4)
- **Bugfix**: Made downthrown Spears properly hit dash colliders/switches

## Hotfix - it's always the edge cases (2.47.6)
- **Bugfix**: Fix crash with Session Variable Trigger when dying while inside the trigger
- **Addition**: Added Replace Vanilla Spinners Lönn script
- **Bugfix**: Fix bloom strength 0 causing certain foreground stylegrounds to fail to render

## Hotfix - GPU Specific Rendering Bugs My Beloathed (2.47.7)
- **Addition**: 'Bake Outlines' mod option

## StrerryBerry Jerry (2.48.0)
- **Addition**: Custom Winged Berry
- **Addition**: Jumpless Golden Berry

## Wet-Dry Mountain (2.49.0)
- **Addition**: Slider-Controlled Water
- **Addition**: Slider Approach Controller
- **Addition**: Water Color Controller
- **Improvement**: Spears now interact with Water

