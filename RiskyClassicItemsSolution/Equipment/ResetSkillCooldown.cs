using BepInEx.Configuration;
using ClassicItemsReturns.Items.Uncommon;
using ClassicItemsReturns.Modules;
using R2API;
using RiskyMod.Items.DLC2;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ClassicItemsReturns.Equipment
{
    internal class ResetSkillCooldown : EquipmentBase<ResetSkillCooldown>
    {
        public override string EquipmentName => "Gigantic Amethyst";
        public override string EquipmentLangTokenName => "RESETSKILLCOOLDOWN";

        public override bool EnigmaCompatible { get; } = true;
        public override bool CanBeRandomlyTriggered { get; } = true;

        public override GameObject EquipmentModel => LoadItemModel("Crystal");

        public override Sprite EquipmentIcon => LoadItemSprite("Crystal");

        public override float Cooldown => 12f;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var dict = new ItemDisplayRuleDict();

            //Don't set up displays if 3d model isn't available
            GameObject display = EquipmentModel;
            if (!display.name.Contains("mdl3d")) return dict;

            dict.Add("EquipmentDroneBody", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EquipmentModel,
                    childName = "GunBarrelBase",
                    localPos = new Vector3(0F, 0F, 2F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });

            return dict;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            ClassicItemsReturnsPlugin.onFinishScanning += CreateCraftableDef;
            Hooks();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot && slot.characterBody && slot.characterBody.skillLocator)
            {
                slot.characterBody.skillLocator.ApplyAmmoPack();
                Util.PlaySound("Play_env_hiddenLab_laptop_activate", slot.gameObject);
                return true;
            }
            return false;
        }

        protected override void CreateCraftableDef()
        {
            CraftableDef craftable = ScriptableObject.CreateInstance<CraftableDef>();
            craftable.pickup = EquipmentDef;
            craftable.recipes = new Recipe[]
            {
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonus.asset").WaitForCompletion()
                        },
                        new RecipeIngredient()
                        {
                            pickup = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/EquipmentMagazine/EquipmentMagazine.asset").WaitForCompletion()
                        }
                    }
                }
            };
            (craftable as ScriptableObject).name = "cdAmethyst";
            PluginContentPack.craftableDefs.Add(craftable);
        }
    }
}