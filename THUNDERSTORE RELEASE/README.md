# ClassicItemsReturns

This mod adds items and equipment from Risk of Rain and Risk of Rain Returns.

| Icon | Item | Desc |
|:--:|:--:|--|
| Common | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicRoot.png) | Bitter Root | Increase `base health regeneration`by `3 hp/s` for `3s` `(+3s per stack)` after killing an enemy. `Scales with level`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicFireShield.png) | Fire Shield | Chance on taking damage to `ignite` all enemies within `12m` for `200%` base damage. Additionally, enemies `burn` for `200%` `(+200% per stack)` base damage. Chance increases the more damage you take.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicPig.png) | Life Savings | Generate `1 gold` `(+1 gold per stack)` every `3 seconds`. Scales over time.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicVial.png) | Mysterious Vial | Increase `base health regeneration` by `+1 hp/s` `(+1 hp/s per stack)`. `Scales with level`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texWeakenOnContactIcon.png) | The Toxin | `Touching` an enemy makes it `vulnerable` to your next attack, `reducing` its `armor` by `20` for `3 seconds` `(+3 seconds per stack)`.

| Icon | Item | Desc |
|:--:|:--:|--|
| Uncommon | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicClover.png) | 56 Leaf Clover | Elite enemies have a `4.5% chance` `(+1.5% chance per stack)` to `drop an item` on death.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicArmsRace.png) | Arms Race | Drones periodically fire a barrage of `4` `(+2 per stack)` missiles that deal `200%` damage each. Recharges every `10` seconds.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicGoldGun.png) | Golden Gun | Deal `extra damage` based on held `gold`, up to an extra `+40% damage` `(+20% per stack)` at `300 gold` `(+150 per stack, scaling with time)`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicShackles.png) | Prison Shackles | `Shackle` enemies on hit for `-30% attack speed` for `2s` `(+2s per stack)`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicPurse.png) | Smart Shopper | Enemies drop an extra `25%` `(+25% per stack)` gold on kill.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicBear.png) | Tough Times | Grants `+14 armor` `(+14 per stack)`.

| Icon | Item | Desc |
|:--:|:--:|--|
| Legendary | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicIceCube.png) | Permafrost | `5%` `(+5% per stack)` chance on hit to `freeze` for `2 seconds`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicThallium.png) | Thallium | On hit, `5% chance` to `poison` for `500% of victim's damage` and slow by `100% movement speed` over `3 seconds` `(+1.5 seconds per stack)`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicSkullRing.png) | Wicked Ring | `Reduce skill cooldowns` by `1s` `(+0.5s per stack)` on kill.

| Icon | Item | Desc |
|:--:|:--:|--|
| Equipment | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicJarSouls.png) | Jar of Souls | Creates `3 ghost duplicates` of an enemy with `100% damage`. `Large` enemies are duplicated `once`. Lasts `30 seconds`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicPills.png) | Prescriptions | Enter a `drug-induced frenzy` for `8` seconds, increasing `damage` by `20%` and `attack speed` by `40%`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicCrystal.png) | Gigantic Amethyst | Activation `refreshes` all cooldowns.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicSquib.png) | Thqwibs | Release a bloom of `12` Thqwibs, detonating on impact for `360% damage` each.

| Icon | Item | Desc |
|:--:|:--:|--|
| Lunar Equipment | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texLostDollIcon.png) | Lost Doll | Sacrifice `33% of your current health` to `damage` an enemy for `400% of your maximum health`.

## Todo

- Update sprites to Returns sprites when it releases.
- More Items?

## Credits

- LostInTransit: Some item implementations
- ClassicItems: Some item implementations
- DestroyedClone: Coding
- Moffein: Coding and Unity stuff
- KomradeSpectre: ItemModBoilerplate
- whitedude: Artifact of Clovers

## Changelog

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
