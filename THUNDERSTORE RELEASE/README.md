# ClassicItemsReturns
**BETA RELEASE**

This mod adds items and equipment from Risk of Rain / Returns. 

| Icon | Item | Desc |
|:--:|:--:|--|
| Common | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicRoot.png) | Bitter Root | Increase `base health regeneration` by `3hp/s` for `3s (+3s per stack)` after killing an enemy.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconFireShield.png) | Fire Shield | Chance on taking damage to `ignite` all enemies within `12m` for `200%` base damage. Additionally, enemies `burn` for `%200%` `(+200% per stack)` base damage. Chance increases the more damage you take.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicPig.png) | Life Savings | Generate `1 gold` `(+1 gold per stack)` every `3 seconds`. Increases over time.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicVial.png) | Mysterious Vial | Increase `base health regeneration` by `+1 hp/s` `(+1 hp/s per stack)`. `Scales with level`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texWeakenOnContactIcon.png) | The Toxin | `Touching` an enemy makes it `vulnerable` to your next attack, `reducing` its `armor` by `20` for `3 seconds` `(+3 seconds per stack)`.

| Icon | Item | Desc |
|:--:|:--:|--|
| Uncommon | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicClover.png) | 56 Leaf Clover | Elite enemies have a `4.5% chance` `(+1.5% chance per stack)` to `drop an item` on death.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicArmsRace.png) | Arms Race | Drones gain a `missile barrage` every `10 seconds`, firing `4 missiles` `(+4 missiles per stack)` for `200% damage.`
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicGoldGun.png) | Golden Gun | Deal `extra damage` based on held `gold`, up to an extra `+40% damage` `(+20% per stack)` at `300 gold` `(+150 per stack, scaling with time)`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicShackles.png) | Prison Shackles | `Shackle` enemies on hit for `-30% attack speed` for `2s` `(+2s per stack)`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicBear.png) | Tough Times | Grants `+14 armor` `(+14 per stack)`.

| Icon | Item | Desc |
|:--:|:--:|--|
| Legendary | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicIceCube.png) | Permafrost | `5%` `(+5% per stack)` chance on hit to `freeze` for `2 seconds`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/Art%20Assets/items/texIconClassicThallium.png) | Thallium | On hit, `5% chance` to `poison` for `500% of victim's damage` and slow by `100% movement speed` over `3 seconds` `(+1.5 seconds per stack)`.

| Icon | Item | Desc |
|:--:|:--:|--|
| Equipment | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texSoulJarIcon.png) | Jar of Souless | `Duplicates` an enemy as a `ghost` with `100% damage`. `Common` enemies are `duplicated 3 times`. Lasts `30 seconds`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texPrescriptionsIcon.png) | Prescriptions | You enter a `drug-induced frenzy` for `8` seconds. Increases `damage` by `20%` and `attack speed` by `40%`.
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texGiganticAmethystIcon.png) | Gigantic Amethyst | Activation `refreshes` all cooldowns.

| Icon | Item | Desc |
|:--:|:--:|--|
| Lunar Equipment | | |
| ![](https://raw.githubusercontent.com/DestroyedClone/RiskyClassicItems/master/RiskyClassicItemsUnityProject/Assets/Sprites/Icons/texLostDollIcon.png) | Lost Doll | Sacrifice `33% of your current health` to `damage` an enemy for `400% of your maximum health`.

### Suggestions



### Credits

- LostInTransit - Some item implementations
- ClassicItems - Some item implementations
- DestroyedClone - Coding
- Moffein - Coding and Unity stuff, Consultation, testing
- KomradeSpectre - ItemModBoilerplate

### Changelog

- `0.2.0`
	- General
		- Major visual overhaul for everything.
		- Updated default AI blacklist settings.
	- Items
		- Bitter Root
			- Reworked version is now enabled by default.
			- Classic Version: Increased HP from 7% to 8% to match RoR1.
		- The Toxin
			- Increased tickrate from 4-8 -> 8-12
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
