# ScugHelper
a few new entities to mess with :3


- brass berry + brass berry block - like a golden berry but you do not reset after dying and you keep the berry, and it continues through save and quit; the berry alone is kind of useless but it can be interesting paired with the blocks

- pinball boosters - boosters (the red and green bubbles from vanilla) that bounce instead of popping when they hit a wall/spring/bumper/what have you - this is really silly with the red ones since those never stop

- booster barrier - solid for only the player if and only if said player is currently in a booster, can be made invisible but is visible by default

- cycler - invisible entity that spins an invisible bar with a configurable RPM, radius, and phase, and has another entity attached at the end specified by ID - you can chain these to get some cool shit goin on with a fourier transform if you want

- rebound block - acts like the power box in farewell in that it `.Rebound()`'s the player when dashed into, but it does not break - can be configured to give no dash, one dash, or two dashes when rebounding, but always gives back stamina (art by [adenator](https://gamebanana.com/members/2012246))

- gripwall - attached to a wall, prevents all vertical movement when wall with it is grabbed but can give back dash/stamina (can give either, both, or neither, configurable)

- debug view trigger - trigger that forces debug drawing (i.e. showing hitboxes) when the player stands in it
 
- speedcheck gate - invisible entity that checks whether the player passes through it via line intersections and does something when they do

also fixes player seekers not being able to hit dash switches

uses some code from Spring Collab 2020, see `LICENSE-SC2020`

# Gallery
<img width="442" height="628" alt="Screenshot_20260329_003547" src="https://github.com/user-attachments/assets/0b229829-fefd-433a-bc0d-77c87a9ea1c6" />
<img width="1520" height="690" alt="Screenshot_20260329_003512" src="https://github.com/user-attachments/assets/6ea6510d-57df-4951-a15f-3836043719c2" />
<img width="1495" height="800" alt="Screenshot_20260329_003448" src="https://github.com/user-attachments/assets/306fcc14-e522-4072-afbc-c373127d9b46" />
<img width="1640" height="893" alt="Screenshot_20260329_003414" src="https://github.com/user-attachments/assets/1956263c-47ea-410e-b17e-57e584b2abd3" />
<img width="1495" height="829" alt="Screenshot_20260329_003300" src="https://github.com/user-attachments/assets/301671f3-8a7e-4744-8b90-1a5e8eba39cc" />

