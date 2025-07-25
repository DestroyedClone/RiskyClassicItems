- `3.4.1`
	- Fixed Snake Eyes crit damage not scaling with buff stacks and being applied without the buff.

- `3.4.0`
	- Fixed Non-EN languages being broken (Thanks Viliger!)

- `3.3.3`
	- Fixed bug with Korean TL.

- `3.3.2`
	- Updated Snake Eyes description in Korean TL.

- `3.3.1`
	- Life Savings
		- Gold value is now based on stage entry difficulty.
		- Added config option to add the On Teleporter Activated condition to this item. (Disabled by default)
		
	- Snake Eyes
		- Extra item stacks give +5% crit damage (up to +30%) instead of extra crit chance. First stack gives
			- 0% crit damage on first stack.
		
		*Fixes the problem where it was too easy to reach 100% crit, and also gives a reason to hold on to this item when crit is maxed.*
		
	- Hit List
		- Fixed marker visual not showing for real this time.

- `3.3.0`
	- Mortar Tube (Green) *Thanks Rob and FORCED_REASSEMBLY!*
		- 10% chance to fire a mortar in an arc for 300% damage.
			- Mortar does not proc items, and has a 10m blast radius with no falloff.
		
	*Reason this is in the Green tier instead of White is because RoR2's White item pool is in a good spot in terms of item distribution, while the Green item pool is skewed too much towards utility/useless items. Goal for this is to be an AtG sidegrade: Less reliable, but with the potential for multihits and AoE.*
	
	- Royal Medallion
		- Attempted to fix the "Drone Buff Sharing" feature not working. (Untested)

- `3.2.5`
	- Mu Construct and Arcane Blades now work on ???

- `3.2.4`
	- Recompiled for MemOp, should fix issues with enemies sometimes not spawning.

- `3.2.3`
	- Fixed an edge case where Classified Access Codes could endlessly attempt to place the interactable.

- `3.2.2`
	- Fixed Hit List marker instantly disappearing. (Didn't actually fix it)

- `3.2.1`
	- Fixed for SotS Phase 3.

- `3.2.0`
	- Fire Shield
		- Reduced activation threshold from 8% -> 5%
		- Added extra activation conditions
			- Shield Broken
			- Out of Danger lost
			
		*This item's activation condition made it useless if you're playing well. New activation conditions and lower threshold should give this more chances to trigger.*
		
	- Snake Eyes
		- Halcyonite Shrine now procs Snake Eyes teamwide.
		- Shrine of the Mountain now procs Snake Eyes teamwide.
		- Buff no longer persists on the following stages:
			- Moon2: Pillars now proc Snake Eyes teamwide.
			- Gilded Coast: Beacons now proc Snake Eyes teamwide.
			- Void Fields: Cells now proc Snake Eyes teamwide.
			- Void Locus: Signals now proc Snake Eyes teamwide.
			- Meridian: Geodes now proc Snake Eyes teamwide.
		
		*Buffs for multiplayer where shrine count doesn't keep up with playercount.*

- `3.1.27`
	- Attempted to fix a Hit List nullref.
	- Life Savings
		- Added config option for time scaling.

- `3.1.26`
	- Removed screenshake from Rusty Jetpack jump VFX.

- `3.1.25`
	- Updated AssistManager compat.

- `3.1.24`
	- Fixed ModifyFinalDamage IL hook for real.

- `3.1.23`
	- Fixed ModifyFinalDamage IL hook.

- `3.1.22`
	- Recompiled for the latest update.
	- Re-enabled Thqwibs since the collision bug got fixed in Vanilla.
		- Increased projectile count from 12 -> 15
		
	- Royal Medallion
		- Increased max active orb entities from 20 -> 40
		- If an AI ally takes a buff, it is also given to the nearest player within 20m.
		
	- Fire Shield
		- Radius increased from 16m -> 20m

- `3.1.21`
	- Recompiled for latest update.

- `3.1.20`
	- Actually updated Toxin damage.

- `3.1.19`
	- Bitter Root
		- Reduced regeneration from +3hp/s -> +2hp/s

	- The Toxin
		- Reduced damage bonus from +30% -> +20%
		
	- Rusty Jetpack
		- Increased jump height from +20% -> +25%
		- Lowered proc sound volume.
		
	- Hyperthreader
		- Increased damage from 60% -> 80%
		- Lowered proc sound volume.

- `3.1.18`
	- Prison Shackles
		- Now actually slows by 40%
			- Old Value: 1/1.4 -> 28.5%

- `3.1.17`
	- Updated KO TL.

- `3.1.16`
	- Added Korean translations (Thanks Dice-001!)
	- Recompiled with latest game dlls.

- `3.1.15`
	- Recompiled for latest AssistManager update.

- `3.1.14`
	- Mu Construct and Arcane Blades now trigger on the False Son's fight.

- `3.1.13`
	- Added R2API_Orb dependency to the Thunderstore manifest. This should fix the mod failing to load on some setups.
	
	- Snake Eyes
		- Buff is preserved when entering Meridian.
		- Now procs on the Geodes.
	
	- Classified Access Codes
		- Added to Meridian.

- `3.1.12`
	- Fixed various features not working if AssistManager isn't installed.
	- Marked Thqwibs as Unfinished until collisions get fixed in Vanilla.

- `3.1.11`
	- Fix for DLC2.
	- Bitter Root
		- Fixed duration reset code not running when triggered via kill assist.

- `3.1.10`
	- Royal Medallion
		- Move speed changed from +2m/s -> +10%
		
		*Base move speed made this uncontrollable when stacked with other movement items.*
		
	- Rusty Jetpack
		- Increased jump bonus from +10% -> +20%

- `3.1.9`
	- Fixed items not fading at close range.

- `3.1.8`
	- Updated models for:
		- 56 Leaf Clover
		- Tough Times
		
		*Thanks hollowman!*

- `3.1.7`
	- Classified Access Codes
		- Fixed interactable being spawned twice.
		
	- Drone Repair Kit
		- Now affects all ally drones so that it works with Equipment Drones.
		- No longer affects non-mechanical allies.
		- Now only spawns a Repair Drone if used by a player.
		
	- Updated models for:
		- Mu Construct
		- Arms Race
		- Smart Shopper
		- Prescriptions
		- Lost Doll
		
		*Thanks hollowman!*
		
- `3.1.6`
	- Added CN TL (Thanks fbjh!)

- `3.1.5`
	- Added PT-BR TL (Thanks kauzok!)

- `3.1.4`
	- Fixed Classified Access Codes interactable not spawning.

- `3.1.3`
	- Repair Drone no longer expires.

- `3.1.2`
	- Fixed Repair Drone giving DroneMeld stacks to Gunner Drones.
	- Fixed spinning Gunner Turret Arms Race displays.

- `3.1.1`
	- Fixed typo in Classified Access Codes lore.

- `3.1.0`

	*Huge thanks to FORCED_REASSEMBLY for the models! Haven't had much time to test, so report any bugs if they arise.*

	- Items
		- Energy Cell (New)
			- Increases attack speed by 10%-40% (+10%-40%) based on health lost.
				- 100% HP = 10%
				- 25% HP = 40%
			
		- Rusty Jetpack (New)
			- Increase jump height by 10% (+10%), and gain a short airhop.
				- Airhop is not affected by jump height, so that you can always use it to break your fall.
			
			*Went with airhop instead of "hold to descend" due to potential conflicts with characters that use that input.*
			
		- Royal Medallion (New)
			- 10% chance on hitting a boss monster to drop a buffing orb that improves health regen, attack speed, move speed, and base damage for 10s (+6s per stack).
				- Gives +3% base damage, +6% attack speed, +2m/s move speed, +1 health regen (scales with level)
				- Can stack up to 10 buffs at a time.
				- New buff stacks reset the duration of existing buff stacks.
			
		- Hyper-Threader (New)
			- 100% chance on hit to fire a laser that deals 60% base damage and bounces to 2 (+1) enemies.
			
		- The Hit List (New)
			- Randomly marks up to 1 (+1 per stack) enemy. Killing a marked enemy permanently increases base damage by 0.5
				- Kills count for everyone holding the item.
				
			*Currently has no limits since theoretically it takes a long time to reach "red-tier damage" with this item. Will keep my eye on this to see if it needs any changes.*
			
		- Classified Access Codes (New)
			- The Atlas Cannon appears each stage, activating deals 40% (+20%) of maximum health as damage to the teleporter boss after it spawns.
				- Nonlinear stacking, check RoRR wiki.
				
			*This is pure jank code, report bugs if they arise.*
			
		- Mu Construct/Arcane Blades
			- Now can trigger from:
				- Moon Battery
				- Void Fields Cell
				- Void Locus Signal
				- Voidling
				- Artifact Reliquary
				- Golden Coast
				- A Moment, Whole
				
		- The Toxin
			- Changed from -30 Armor to +30% Damage Received like in Returns since armor reduction has diminishing returns.
			- Reduced duration from 3s (+3s) -> 3s (+1.5s) since it's really easy to stack due to being a Common item.
			
		- Bitter Root
			- New buff stacks now reset the duration of existing buff stacks.
			
		- 56 Leaf Clover
			- No longer rolls for dead/disconnected players.
			
		- Arms Race
			- Added Drone displays.
			
	- Equipment
		- Added Equipment Drone displays for all equipment.
		- Drone Repair Kit (New)
			- All drones are repaired to full health and empowered for 8s. Summons a unique drone.
				- +50% Attack Speed
				- -50% Cooldowns
				- +50 Armor
			
	- Unfinished Items (Disabled by default)
		- Boxing Glove (New)
			- 6% (+6%) chance on hitting enemies to knock them back for 100% TOTAL damage.

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