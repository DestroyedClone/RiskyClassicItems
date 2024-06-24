- `3.0.1`
	- Adjusted icons for some items to be closer to the in-game model.
	- Fixed Prescriptions description listing the wrong numbers in the readme.

- `3.0.0`
	- Fully implemented models for all items.
		
		*Huge thanks to FORCED_REASSEMBLY and hollowman104 for their hard work!*
		
	- Bitter Root
		- Reduced regen duration from 3s -> 2s
		
		*This puts it in-line with Monster Tooth.*
		
	- Thallium
		- Debuff now only displays 1 stack. (Purely a visual change, internally it's capped at 1 stack per player)
		
	- Permafrost
		- Fixed chance not being affected by Luck.
		- Now scales linearly instead of having Stun Grenade scaling.
		- No longer works on Champions.
			- If RiskyMod is installed, it will apply the Freeze debuff.
		
	- Jar of Souls
		- Increased cooldown from 90s -> 100s
		- Reduced ghost lifetime from 30s -> 25s
		- Ghosts no longer spawn directly inside their source enemy, which should fix the item being able to instakill most flying enemies. There's probably a few edge cases where this si still possible.
		
		*Same stats of The Back-Up as a baseline.*
		
	- Prescriptions
		- Increased attack speed boost from +40% -> +50%
		- Increased damage boost from +20% -> +25%
		
		*I'm under the impression that this doesn't get taken mainly because it's boring rather than because it's weak, but a bit of a buff won't hurt.*
		
	- Lost Doll
		- Reduced cooldown from 45s -> 25s
		- Self-damage is no longer unblockable.
		- Now uses that one unused Mithrix log.
			- Original log available in config.

- `2.0.3`
	- Fixed Arms Race not getting removed from existing minions after scrapping the item.

- `2.0.2`
	
	- Fixed incompatibility with Refightilization.
	
	- Snake Eyes
		- Stacks now persist when entering Hidden Realms and Final Stages.
		
	- Jar of Souls
		- Reduced max active uses count from 3 -> 2
		- Replaced Limit Spawns config with Max Uses config, so you can now specifically set how many active uses of the equipment should be allowed.
		- Added Voidling to default blacklist due to general jankiness. (Existing config unaffected)

- `2.0.1`

	- Added lore.
	- Locked Jewel
		- Fixed the item not proccing on chests due to the Devotion update.
	- Arms Race
		- Reduced missile damage from 200% -> 100%
	- Gigantic Amethyst
		- Reduced cooldown from 20s -> 12s

- `2.0.0`

	- Fixed for Devotion update.

	- Added 3d models for:
		- Razor Penny (WIP)
		- Mysterious Vial (WIP)
		- Life Savings
		- Arcane Blades
		- Fire Shield
		- Snake Eyes
		- Mu Construct
			- TODO: Add follower when active.
		
		- Tough Times
		- Arms Race
		- Golden Gun
		- Locked Jewel
		- Prison Shackles
		- Smart Shopper
		
		- Thallium
		
		- Jar of Souls (WIP)
		- Prescriptions
		- Gigantic Amethyst
		- Thqwibs
		- Lost Doll
		
	- TODO:
		- Bitter Root
		- The Toxin
		- 56 Leaf Clover
		- Permafrost
		- Wicked Ring
		
		***Contact me on the GitHub if you are interested in contributing models or touching up the existing models!***
		
	- The Toxin
		- Increased armor reduction from 20 -> 30
		
		*Keeping stacking behavior different from Returns since pure damage stacking overlaps too much with Focus Crystal.*
		
	- Arms Race
		- Added config option to enable it on non-mechanical allies. (Disabled by default)
		
	- Prison Shackles
		- Increased slow factor from 30 -> 40 (This ends up being a true -30% attack speed)
		- Duration now scales off of proc coefficient.
		- Fixed self-procs on 0 proc coefficient attacks.

	- Thallium
		- Proc chance now scales off of proc coefficient.
		- Fixed self-procs on 0 proc coefficient attacks.

- `1.3.8`
	- Jar of Souls
		- Added body blacklist config option.
			- Default Blacklist: ShopkeeperBody, VoidInfestorBody, WispSoulBody, UrchinTurretBody, MinorConstructAttachableBody
		- Ghosts now decay HP over 30s. Lifetime remains the same, they still die after 30s even if they heal.
			- On Eclipse, Ghosts are spawned at full health if decay is enabled.
			- Can disable this in config to use old behavior where HP would remain constant.
		
			*This will fix most bosses not using their attacks. Clay Dunestriders and Grandparents can teamkill at low health if converted, I believe there are mods on Thunderstore to fix this.*
			
		- Nerfed enemy Scavenger Jar of Souls
			- Lifetime reduced from 30s -> 10s
			- Ghost count reduced from 3 -> 1
		
			*This is just a bandaid fix, I highly recommend using AI Blacklist to disable it.*
			
- `1.3.7`
	- Fire Shield
		- Activation threshold reduced from 10% -> 8% to match up with Personal Shield Generator.
		
	- Wicked Ring
		- Default setting changed to the RoR1 version. (existing config unaffected)
		
	- Jar of Souls
		- Added link to AI Blacklist (https://thunderstore.io/package/Moffein/AI_Blacklist/) to README due to reports of Scavengers receiving this equipment and instant-ending runs.

- `1.3.6`
	- Fixed a nullref with Clover.

- `1.3.5`
	- Fixed a nullref with Golden Gun.

- `1.3.4`
	- Locked Jewel
		- Reduced barrier from 35% (+15%) -> 25% (+15%)

- `1.3.3`
	- Added config to disable Razor Penny in Bazaar. (default: true)

- `1.3.2`
	- Added config to disable Locked Jewel in Bazaar. (default: true)

- `1.3.1`
	- Fixed Jar of Souls being able to target the rocks on Sky Meadow.

- `1.3.0`
	- New items
		- Mu Construct
			- Heal for **2.5%** of your **health** every **5** *(-25% per stack)* seconds after the **Teleporter** has been activated.
		- Locked Jewel
			- Activating an interactable grants **35%** *(+15% per stack)* of your **maximum health** as barrier as well as **$8**. Scales with time.
		
	- Arcane Blades/Mu Construct
		- Now active during Mithrix and Voidling.
		
	- Fixes
		- Fixed Lost Doll being invisible.
		- Fixed Razor Penny being invisible when using Classic sprites.
		
	**Looking for a modeler! Contact us on the Github page if interested!**

- `1.2.1`
	- Fixed missing material for The Toxin.

- `1.2.0`
	- Added Risk of Rain Returns sprites.
		- Classic sprites available in config.
		
	- New Items
		- Razor Penny
			- 5% (+5%) critical chance. Gain $1 (+$1) on crit.
				- Gold does NOT scale with time.

		- Arcane Blades
			- 20% (+20%) movement speed after activating the Teleporter.
				- In Simulacrum, it is active during waves and inactive when the bubble is moving.
		
	- Snake Eyes
		- Now triggers on any shrine use.
		- Increased crit from 7% (+7%) -> 7.5% (+7.5%)
		
		*Failing a Chance Shrine is too conditional and straight up doesn't work with Sacrifice. Will see how the item performs before determining if further buffs are needed.*
		
	- Thallium
		- Buffed chance from 5% -> 10% to match Returns.
		- Buffed damage from 500% total damage  -> 2x300% per second (Returns is 2x500% per second but takes time to ramp up to the full amount)
		
		*I'll figure out how the math actually works later.*
		
	- Permafrost
		- Reduced chance from 5% -> 3%

- `1.1.1`
	- Fixed missing space in Life Savings description.

- `1.1.0`
	- Fire Shield
		- Now is guaranteed to proc after taking more than 10% of your max health, instead of scaling with health lost.
		- Increased Blast Radius from 12m -> 16m
		- Ignite now procs before the blast attack.
		
	- Snake Eyes
		- Same implementation as in RoRR. 7% (+7%) crit chance on Shrine fail, stacks up to 6 times. Resets each map.
			- Persists if you die and get revived on the same stage
	
	- Golden Gun
		- Fixed buff being cleansable by Blast Shower.
		
	- Jar of Souls
		- Boss ghost damage increased from 200% -> 300%
		
		*Noticed most boss ghosts severely underperforming compared to normal mobs.*
	
	- Lost Doll
		- Reduced cost from 33% HP -> 25% HP
		- Activation sound is now networked.
		- Disabled VFX due to concerns about networking (it was set up in a nonstandard way).
			- It only showed for the host, and not clients.
			- Replaced it with a simple placeholder effect for now. Not sure if I'll get around to fixing it fully.

- `1.0.6`
	- Adjusted Enigma/Chaos settings. Previously everything was set to True.
		- Lost Doll
			- Enigma: False
			- Chaos: False
		- Jar of Souls
			- Enigma: True
			- Chaos: False
		- Prescriptions
			- Enigma: True
			- Chaos: True
		- Gigantic Amethyst
			- Enigma: True
			- Chaos: True
		- Thqwibs
			- Enigma: True
			- Chaos: True
	- Thqwibs
		- Reduced cooldown from 60s -> 45s

- `1.0.5`
	- 56 Leaf Clover
		- No longer procs if there is no valid killer.
		- No longer procs on teamkills.

- `1.0.4`
	- Life Savings
		- No longer gives gold when exiting a stage to prevent softlocks.

- `1.0.3`
	- Jar of Souls
		- Blacklisted Void Infestor, Soul Wisp, Malachite Urchin, Newt
		- Blacklisted SS2U Nemesis Bosses (can be re-enabled in the ClassicItemsReturns config)

- `1.0.2`
	- Bitter Root
		- Can now trigger off of assists if RiskyMod is installed.
	- 56 Leaf Clover
		- Enabled Classic chances by default. (Roll for every player instead of only rolling for the killer.)
			- Need to manually change existing config.
	- Jar of Souls
		- Gup/Geep ghosts no longer split on death.

- `1.0.1`
	- Readme fix.

- `1.0.0`
	- General
		- Major visual overhaul for everything.
		- Updated default AI blacklist settings.
	- New Items
		- Added Fire Shield.
		- Added Smart Shopper.
		- Added Wicked Ring.
			- Uses LiT's rework: -1s cooldown on kill.
			- Can enable classic version in config.
		- Added Thqwibs.
		
	- Item Changes
		- Bitter Root
			- Reworked version is now enabled by default.
			- Classic Version: Increased HP from 7% to 8%
		- The Toxin
			- Increased tickrate from 4-8 -> 10
			- Doubled radius, and added a minimum search radius to the contact check.
		- Tough Times
			- Increased armor from 14 (+7) -> 14 (+14)
		- Prison Shackles
			- Disabled proc sound.
	- Equipment
		- Lost Doll
			- Increased proc coefficient from 0 -> 1.0
			- Reduced damage from 5x HP -> 4x HP
			- Increased HP cost from 25% -> 33%
	- Artifacts
		- Added Artifact of Clover
			- Players start with a single 56 Leaf Clover in their inventory. (Requires 56 Leaf Clover to be enabled in config)


- `0.1.0`
	- Release