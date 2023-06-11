using R2API;
using RoR2;
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
                false, true, false, Assets.nseShackles);
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