using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Items
{
    public class FilialImprinting : ItemBase<FilialImprinting>
    {
        public override string ItemName => "FilialImprinting";

        public override string ItemLangTokenName => ItemName.ToUpper();

        public float cooldown = 20f;
        public float damageCoefficient = 1f;
        public float movespeedCoefficient = 1f;
        public float regenBoost = 1;

        public override string[] ItemFullDescriptionParams => new string[]
        {
            cooldown.ToString(),
            (damageCoefficient * 100).ToString(),
            (movespeedCoefficient * 100).ToString(),
            regenBoost.ToString(),
        };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => LoadModel();

        public override Sprite ItemIcon => LoadSprite();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
        }

        public class FilialImprintingBehavior : MonoBehaviour
        {
            public CharacterBody ownerBody;
            public BuffDef damageBuff => RoR2Content.Buffs.FullCrit;
            public BuffDef regenBuff => RoR2Content.Buffs.CrocoRegen;
            public BuffDef movespeedBuff => RoR2Content.Buffs.CloakSpeed;

            public float stopwatch = 0;
            public float cooldown => FilialImprinting.Instance.cooldown;

            public void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0)
                {
                    stopwatch = cooldown;
                }
            }

            public void DropRandomBuff()
            {
                if (Util.CheckRoll(33))
                {
                    DropBuff(regenBuff);
                    return;
                }
                if (Util.CheckRoll(33))
                {

                    DropBuff(regenBuff);
                    return;
                }
                if (Util.CheckRoll(33))
                {

                    DropBuff(regenBuff);
                    return;
                }
            }
            public void DropBuff(BuffDef buff)
            {

            }
        }
    }
}