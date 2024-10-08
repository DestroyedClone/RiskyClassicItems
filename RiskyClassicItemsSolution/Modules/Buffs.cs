﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

//TODO: Need consistency. Should buffs be entirely defined here, or defined in their respective items?
namespace ClassicItemsReturns.Modules
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
        public static BuffDef WeakenOnContactBuff;
        public static BuffDef DroneRepairBuff;
        public static BuffDef RoyalMedallionBuff;

        public static BuffDef HitListEnemyMarker;
        public static BuffDef HitListPlayerBuff;

        //Unique icon for each buff.
        public static class SnakeEyesBuffs
        {
            public static BuffDef Snake1, Snake2, Snake3, Snake4, Snake5, Snake6;
        }

        public static void Initialize()
        {
            InitializeBuffDefs();

            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Weak")
                    ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || self.HasBuff(WeakenOnContactBuff);
                    });
                }
                else
                {
                    Debug.LogError("ClassicItemsReturns: Failed to set up Toxin VFX.");
                }

                if (c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "CrocoRegen")
                    ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || self.HasBuff(BitterRootBuff);
                    });
                }
                else
                {
                    Debug.LogError("ClassicItemsReturns: Failed to set up Bitter Root.");
                }
            };
            IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Weak")
                    ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || (self.body.HasBuff(WeakenOnContactBuff));
                    });
                }
                else
                {
                    Debug.LogError("ClassicItemsReturns: Failed to set up Toxin Overlay.");
                }

                if (c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "FullCrit")
                    ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || (self.body.HasBuff(DrugsBuff));
                    });
                }
                else
                {
                    Debug.LogError("ClassicItemsReturns: Failed to set up Prescriptions Overlay.");
                }

                if (c.TryGotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Immune")
                    ))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                    {
                        return hasBuff || (self.body.HasBuff(DroneRepairBuff));
                    });
                }
                else
                {
                    Debug.LogError("ClassicItemsReturns: Failed to set up Drone Repair Kit Overlay.");
                }
            };
        }

        private static void InitializeBuffDefs()
        {
            DrugsBuff = CreateBuffInternal("CIR_Prescriptions",
                            Color.white,
                            false,
                            null,
                            Modules.Assets.LoadSprite("texBuffPills"),
                            false,
                            false,
                            false,
                            null);
            BitterRootBuff = CreateBuffInternal("CIR_BitterRoot",
                new Color(0.784f, 0.937f, 0.427f, 1f), true,
                null,
                Modules.Assets.LoadSprite("texBuffGinseng"),
                false,
                false,
                false,
                null);
            GoldenGunBuff = CreateBuffInternal("CIR_GoldenGun",
                new Color32(219, 211,  77, 255), true,
                null, Modules.Assets.LoadSprite("texBuffGoldGun_nomoney"),
                false, false,
                false, null);
            PermafrostChilledBuff = CreateBuffInternal("CIR_Chilled",
                Color.cyan, false,
                null, Modules.Assets.LoadSprite("texPermafrostBuffIcon"),   //whoops forgot this. If it's unused no one's going to notice the missing icon.
                false, true,
                false, null);//Play_wFeralShoot2

            ShacklesBuff = CreateBuffInternal("CIR_PrisonShackles",
                new Color(0.918f, 0.408f, 0.420f, 1.000f), false,
                null, Modules.Assets.LoadSprite("texBuffShackles"),
                false, true, false, null);

            ThalliumBuff = CreateBuffInternal("CIR_ThalliumBuff",
                rgb(123, 74, 149), false,
                null, Modules.Assets.LoadSprite("texBuffThallium"),
                false, true, false, null);

            WeakenOnContactBuff = CreateBuffInternal("CIR_WeakenOnContact",
                new Color(0.784f, 0.937f, 0.427f, 1f), false,
                null, Modules.Assets.LoadSprite("texBuffToxin"),
                false, true, false, null);

            DroneRepairBuff = CreateBuffInternal("CIR_DroneRepairBuff",
                new Color(0.784f, 0.937f, 0.427f, 1f), false,
                null, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ShockNearby/texBuffTeslaIcon.tif").WaitForCompletion(),
                false, true, false, null);

            SnakeEyesBuffs.Snake1 = CreateBuffInternal("CIR_Snake1",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake1"),
                false, false,
                false, null);
            SnakeEyesBuffs.Snake2 = CreateBuffInternal("CIR_Snake2",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake2"),
                false, false,
                false, null);
            SnakeEyesBuffs.Snake3 = CreateBuffInternal("CIR_Snake3",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake3"),
                false, false,
                false, null);
            SnakeEyesBuffs.Snake4 = CreateBuffInternal("CIR_Snake4",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake4"),
                false, false,
                false, null);
            SnakeEyesBuffs.Snake5 = CreateBuffInternal("CIR_Snake5",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake5"),
                false, false,
                false, null);
            SnakeEyesBuffs.Snake6 = CreateBuffInternal("CIR_Snake6",
                new Color32(214, 58, 58, 255), false,
                null, Modules.Assets.LoadSprite("texBuffSnake6"),
                false, false,
                false, null);

            HitListEnemyMarker = CreateBuffInternal("CIR_HitListEnemyMarker",
                new Color32(255, 232, 68, 255), false,
                null, Modules.Assets.LoadSprite("texBuffHitListMark"),
                false, false,
                false, null);

            HitListPlayerBuff = CreateBuffInternal("CIR_HitListPlayerBuff",
                new Color32(255, 232, 68, 255), true,
                null, Modules.Assets.LoadSprite("texBuffHitListMark"),
                false, false,
                false, null);

            RoyalMedallionBuff = CreateBuffInternal("CIR_RoyalMedallionBuff",
                new Color32(255, 232, 68, 255), true,
                null, Addressables.LoadAssetAsync<Sprite>("RoR2/Base/WarCryOnMultiKill/texWarcryBuffIcon.tif").WaitForCompletion(),
                false, false,
                false, null);
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

        //https://github.com/prodzpod/TemplarSkins/blob/master/TemplarSkins/Skins.cs#L52
        public static Color rgb(byte r, byte g, byte b, byte a = 255)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}