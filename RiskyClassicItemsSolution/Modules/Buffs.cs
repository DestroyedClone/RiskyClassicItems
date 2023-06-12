using R2API;
using RoR2;
using System;
using UnityEngine;

namespace RiskyClassicItems.Modules
{
    internal class Buffs
    {
        public static BuffDef DrugsBuff;
        public static BuffDef SnowglobeBuff;
        public static BuffDef BitterRootBuff;
        public static BuffDef GoldenGunBuff;
        public static BuffDef PermafrostChilledBuff;
        public static BuffDef ShacklesBuff;
        public static BuffDef ThalliumBuff;

        public static void Initialize()
        {
            InitializeBuffDefs();


            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        }

        private static void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);
            void AddOverlays(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount >= CharacterModel.maxOverlays)
                {
                    return;
                }
                if (condition)
                {
                    Material[] array = self.currentOverlays;
                    int num = self.activeOverlayCount;
                    self.activeOverlayCount = num + 1;
                    array[num] = overlayMaterial;
                }
            }
            if (self.body)
            {
                AddOverlays(CharacterModel.fullCritMaterial, self.body.HasBuff(DrugsBuff));
            }
        }

        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            //an NSE would play it every time it's applied, which gets too loud from sound stacking
            //so better to play on first stack
            if (buffDef == ShacklesBuff)
            {
                Util.PlaySound("Play_gravekeeper_attack2_shoot_singleChain", self.gameObject);
            }
        }

        private static void InitializeBuffDefs()
        {
            DrugsBuff = CreateBuffInternal("RCI_Prescriptions",
                            Color.red,
                            false,
                            null,
                            Assets.LoadSprite("texPrescriptionsBuffIcon"),
                            false,
                            false,
                            false,
                            null);
            BitterRootBuff = CreateBuffInternal("RCI_BitterRoot",
                Color.green,
                false,
                null,
                Assets.LoadSprite("texBitterRootBuffIcon"),
                false,
                false,
                false,
                null);
            GoldenGunBuff = CreateBuffInternal("RCI_GoldenGun",
                Color.yellow, true,
                null, Assets.LoadSprite("texPrescriptionsBuffIcon"),
                true, false,
                false, null);
            PermafrostChilledBuff = CreateBuffInternal("RCI_Chilled",
                Color.cyan, false,
                null, Assets.LoadSprite("texPermafrostBuffIcon"),
                false, true,
                false, null);//Play_wFeralShoot2

            ShacklesBuff = CreateBuffInternal("RCI_PrisonShackles",
                Color.blue, false,
                null, Assets.LoadSprite("texPrisonShacklesBuffIcon"),
                false, true, false, null);
            ThalliumBuff = CreateBuffInternal("RCI_ThalliumBuff",
                Color.green, true,
                null, Assets.LoadSprite("texThalliumBuffIcon"),
                false, true, false, null);
        }

        public static BuffDef CreateBuffInternal(string name, Color buffColor, bool canStack, EliteDef eliteDef, Sprite iconSprite, bool isCooldown, bool isDebuff, bool isHidden, NetworkSoundEventDef startSfx)
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.eliteDef = eliteDef;
            buffDef.iconSprite = iconSprite;
            buffDef.isCooldown = isCooldown;
            buffDef.isDebuff = isDebuff;
            buffDef.isHidden = isHidden;
            buffDef.startSfx = startSfx;
            buffDef.name = name;
            ContentAddition.AddBuffDef(buffDef);
            return buffDef;
        }

        public static BuffDef CreateEliteBuffDef(string name, Color buffcolor, EliteDef eliteDef, Sprite iconSprite, NetworkSoundEventDef startSfx)
        {
            return CreateBuffInternal(name, buffcolor, false, eliteDef, iconSprite, false, false, false, startSfx);
        }
    }
}