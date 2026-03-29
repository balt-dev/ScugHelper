# ScugHelper
a few new entities to mess with :3


- brass berry + brass berry block - like a golden berry but you do not reset after dying and you keep the berry, and it continues through save and quit; the berry alone is kind of useless but it can be interesting paired with the blocks

- pinball boosters - boosters (the red and green bubbles from vanilla) that bounce instead of popping when they hit a wall/spring/bumper/what have you - this is really silly with the red ones since those never stop

- booster barrier - solid for only the player if and only if said player is currently in a booster, can be made invisible but is visible by default

- cycler - invisible entity that spins an invisible bar with a configurable RPM, radius, and phase, and has another entity attached at the end specified by ID - you can chain these to get some cool shit goin on with a fourier transform if you want

- rebound block - acts like the power box in farewell in that it `.Rebound()`'s the player when dashed into, but it does not break - can be configured to give no dash, one dash, or two dashes when rebounding, but always gives back stamina (art by [adenator](https://gamebanana.com/members/2012246))

- gripwall - attached to a wall, prevents all vertical movement when wall with it is grabbed but can give back dash/stamina (can give either, both, or neither, configurable)

- debug view trigger - trigger that forces debug drawing (i.e. showing hitboxes) when the player stands in it



uses some code from Spring Collab 2020, see `LICENSE-SC2020`
