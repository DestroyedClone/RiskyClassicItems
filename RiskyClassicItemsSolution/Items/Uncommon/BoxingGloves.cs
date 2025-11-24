using BepInEx.Configuration;
using R2API;
using ClassicItemsReturns.Modules;
using RoR2;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Items.Uncommon
{
    public class BoxingGloves : ItemBase<BoxingGloves>
    {
        public float chance = 6f;
        public float chanceStack = 6f;
        public float damageMult = 1f;

        public float baseForce = 2800f;
        public float airbornMultiplier = 0.7f;
        public float championMultiplier = 0.7f;

        public static GameObject procEffect;

        public override string ItemName => "Boxing Gloves";

        public override string ItemLangTokenName => "BOXINGGLOVES";

        public override object[] ItemFullDescriptionParams => new object[]
        {
            chance,
            chanceStack,
            damageMult * 100
        };

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => LoadItemModel("Boxing");

        public override Sprite ItemIcon => LoadItemSprite("Boxing");

        public override ItemTag[] ItemTags => new ItemTag[]
        {
            ItemTag.Damage,
            ItemTag.Utility,
            ItemTag.CanBeTemporary
        };

        public override bool Unfinished => true;

        public override void CreateAssets(ConfigFile config)
        {
            procEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXLarge.prefab").WaitForCompletion()
                .InstantiateClone("CIR_BoxingGloveImpactEffect", false);
            EffectComponent ec = procEffect.GetComponent<EffectComponent>();
            ec.soundName = "Play_ClassicItemsReturns_Punch";
            PluginContentPack.effectDefs.Add(new EffectDef(procEffect));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            SneedHooks.ProcessHitEnemy.OnHitAttackerActions += OnHit;
        }

        private void OnHit(DamageInfo damageInfo, CharacterBody victimBody, CharacterBody attackerBody)
        {
            if (!attackerBody.inventory) return;
            int itemCount = attackerBody.inventory.GetItemCountEffective(ItemDef);
            if (itemCount <= 0) return;

            float chance = Utils.ItemHelpers.StackingLinear(itemCount, this.chance, chanceStack);
            if (!Util.CheckRoll(chance * damageInfo.procCoefficient, attackerBody.master)) return;


            //Reset Victim Force
            //Disabled since it has the potential to conflict with physics-based attacks
            /*if (victimBody.rigidbody)
            {
                victimBody.rigidbody.velocity = new Vector3(0f, victimBody.rigidbody.velocity.y, 0f);
                victimBody.rigidbody.angularVelocity = new Vector3(0f, victimBody.rigidbody.angularVelocity.y, 0f);
            }
            if (victimBody.characterMotor != null)
            {
                victimBody.characterMotor.velocity.x = 0f;
                victimBody.characterMotor.velocity.z = 0f;
                victimBody.characterMotor.rootMotion.x = 0f;
                victimBody.characterMotor.rootMotion.z = 0f;
            }*/

            //Scale Force to Mass
            Vector3 attackForce = baseForce * (damageInfo.position
                - damageInfo.attacker.transform.position).normalized;

            if (victimBody.isChampion)
            {
                attackForce *= championMultiplier;
            }

            if (victimBody.isFlying || victimBody.characterMotor && !victimBody.characterMotor.isGrounded)
            {
                attackForce *= airbornMultiplier;
            }

            if (victimBody.rigidbody)
            {
                float forceMult = Mathf.Max(victimBody.rigidbody.mass / 100f, 1f);
                if (victimBody.isFlying) forceMult = Mathf.Min(forceMult, 7.5f);

                attackForce *= forceMult;
            }

            DamageInfo bgDamageInfo = new DamageInfo()
            {
                attacker = damageInfo.attacker,
                canRejectForce = false,
                crit = damageInfo.crit,
                damage = damageInfo.damage * damageMult,
                damageColorIndex = DamageColorIndex.Item,
                damageType = DamageType.Generic,
                dotIndex = DotController.DotIndex.None,
                force = attackForce,
                inflictor = damageInfo.attacker,
                position = damageInfo.position,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 0f
            };
            victimBody.healthComponent.TakeDamage(bgDamageInfo);

            EffectManager.SpawnEffect(procEffect, new EffectData
            {
                origin = damageInfo.position,
                scale = 8f
            }, true);
        }
    }
}