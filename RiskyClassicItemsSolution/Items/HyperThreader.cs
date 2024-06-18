using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using RoR2.Orbs;
using System;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Items
{//https://github.com/swuff-star/LostInTransit/blob/0fc3e096621a2ce65eef50f0e82db125c0730260/LIT/Assets/LostInTransit/Modules/Pickups/Items/Thallium.cs
    public class HyperThreader : ItemBase<HyperThreader>
    {
        float chance = 100;
        float damageCoeff = 0.4f;
        float bounceRange = 40f;
        int bounceCount = 2;
        int bounceCountPerStack = 1;
        public static GameObject orbEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/ChainGunOrbEffect.prefab").WaitForCompletion();

        public override string ItemName => "Hyper-Threader";

        public override string ItemLangTokenName => "HYPERTHREADER";

        public override object[] ItemFullDescriptionParams => new object[]
        {
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Thallium");

        public override Sprite ItemIcon => LoadItemSprite("Thallium");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.AIBlacklist
        };

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override void CreateAssets(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SharedHooks.OnHitEnemy.OnHitEnemyAttackerInventoryAndVictimBodyActions += HyperThreaderOnHit;
        }

        private void HyperThreaderOnHit(GlobalEventManager globalEventManager, DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody, Inventory attackerInventory)
        {
            int itemCount = attackerInventory.GetItemCount(ItemDef);
            if (itemCount <= 0) return;

            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;

            int calcBounceCount = Utils.ItemHelpers.StackingLinear(itemCount, this.bounceCount, bounceCountPerStack);
            ChainGunOrb chainGunOrb = new ChainGunOrb(orbEffect);
            chainGunOrb.damageValue = attackerBody.damage * damageCoeff;
            chainGunOrb.isCrit = attackerBody.RollCrit();
            chainGunOrb.teamIndex = attackerBody.teamComponent.teamIndex;
            chainGunOrb.attacker = attackerBody.gameObject;
            chainGunOrb.procCoefficient = 0f;
            chainGunOrb.procChainMask = default;//damageInfo.procChainMask;
            chainGunOrb.origin = attackerBody.inputBank.aimOrigin;
            chainGunOrb.speed = 600f;   //Drone Parts is 600f
            chainGunOrb.bouncesRemaining = calcBounceCount;
            chainGunOrb.bounceRange = bounceRange;
            chainGunOrb.damageCoefficientPerBounce = 1f;
            chainGunOrb.targetsToFindPerBounce = 1;
            chainGunOrb.canBounceOnSameTarget = false;
            chainGunOrb.damageColorIndex = DamageColorIndex.Default;

            chainGunOrb.target = victimBody.mainHurtBox;
            OrbManager.instance.AddOrb(chainGunOrb);
        }
    }
}