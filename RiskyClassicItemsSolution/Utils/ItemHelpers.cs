using ClassicItemsReturns.Modules;
using ClassicItemsReturns.Utils.Components;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static ClassicItemsReturns.Utils.Components.MaterialControllerComponents;

namespace ClassicItemsReturns.Utils
{
    public static class ItemHelpers
    {
        private static Shader hopooShader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
        private static List<Material> cachedMaterials = new List<Material>();

        /// <summary>
        /// A helper that will set up the RendererInfos of a GameObject that you pass in.
        /// <para>This allows it to go invisible when your character is not visible, as well as letting overlays affect it.</para>
        /// </summary>
        /// <param name="obj">The GameObject/Prefab that you wish to set up RendererInfos for.</param>
        /// <param name="debugmode">Do we attempt to attach a material shader controller instance to meshes in this?</param>
        /// <returns>Returns an array full of RendererInfos for GameObject.</returns>
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj, bool debugmode = false)
        {
            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                if (debugmode)
                {
                    var controller = AllRenderers[i].gameObject.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                    controller.Renderer = AllRenderers[i];
                }

                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        public static void SetupMaterials(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.material || !(renderer.material.shader && renderer.material.shader.name == "Standard")) continue;

                if (renderer.name == "UseGlassShader")
                {
                    Debug.Log("ClassicItemsReturns: Swapping material to Infusion Glass material for " + model.name);
                    renderer.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Infusion/matInfusionGlass.mat").WaitForCompletion();
                }
                else if (renderer.name == "UseGlass2Shader")
                {
                    Debug.Log("ClassicItemsReturns: Swapping material to Elixir Glass material for " + model.name);
                    renderer.material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/HealingPotion/matHealingPotionGlass.mat").WaitForCompletion();
                }
                else if (renderer.name == "UseGlass3Shader")
                {
                    Debug.Log("ClassicItemsReturns: Swapping material to Vending Machine Glass material for " + model.name);
                    renderer.material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VendingMachine/matVendingMachineGlass.mat").WaitForCompletion();
                }
                else
                {
                    renderer.sharedMaterial = SetHopooMaterial(renderer.sharedMaterial);
                }
            }
        }
        public static Sprite LoadItemSprite(string spriteName)
        {
            Sprite sprite3d = Assets.LoadSprite("texIcon3d" + spriteName);
            Sprite spriteReturns = Assets.LoadSprite("texIcon" + spriteName);
            Sprite spriteClassic = Assets.LoadSprite("texIconClassic" + spriteName);

            if (ClassicItemsReturns.ClassicItemsReturnsPlugin.use3dModels)
            {
                if (sprite3d) return sprite3d;
                if (spriteReturns) return spriteReturns;
                return spriteClassic;
            }
            else
            {
                if (!ClassicItemsReturnsPlugin.useClassicSprites)
                {
                    if (spriteReturns) return spriteReturns;
                    if (spriteClassic) return spriteClassic;
                    return sprite3d;
                }
                else
                {
                    if (spriteClassic) return spriteClassic;
                    if (spriteReturns) return spriteReturns;
                    return sprite3d;
                }
            }
        }

        //Copied from Henry
        public static Material SetHopooMaterial(this Material tempMat)
        {
            if (cachedMaterials.Contains(tempMat))
                return tempMat;

            float? bumpScale = null;
            Color? emissionColor = null;
            float? parallax = null;

            bool hasNormal = tempMat.IsKeywordEnabled("_NORMALMAP");
            bool hasParallax = tempMat.IsKeywordEnabled("_PARALLAXMAP");

            Texture parallaxTex = null;
            Vector2 parallaxTexScale = Vector2.one;
            Vector2 parallaxTexOffset = Vector2.zero;

            Texture bumpTex = null;
            Vector2 bumpTexScale = Vector2.one;
            Vector2 bumpTexOffset = Vector2.zero;


            //grab values before the shader changes
            if (hasParallax)
            {
                parallaxTex = tempMat.GetTexture("_ParallaxMap");
                parallaxTexScale = tempMat.GetTextureScale("_ParallaxMap");
                parallaxTexOffset = tempMat.GetTextureOffset("_ParallaxMap");
                parallax = tempMat.GetFloat("_Parallax");
            }

            if (hasNormal)
            {
                bumpScale = tempMat.GetFloat("_BumpScale");
                bumpTex = tempMat.GetTexture("_BumpMap");
                bumpTexScale = tempMat.GetTextureScale("_BumpMap");
                bumpTexOffset = tempMat.GetTextureOffset("_BumpMap");
            }
            if (tempMat.IsKeywordEnabled("_EMISSION"))
            {
                emissionColor = tempMat.GetColor("_EmissionColor");
            }

            //set shader
            tempMat.shader = hopooShader;

            //apply values after shader is set
            tempMat.SetColor("_Color", tempMat.GetColor("_Color"));
            tempMat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            tempMat.EnableKeyword("DITHER");

            if (hasParallax)
            {
                tempMat.SetTexture("_ParallaxMap", parallaxTex);
                tempMat.SetTextureScale("_ParallaxMap", parallaxTexScale);
                tempMat.SetTextureOffset("_ParallaxMap", parallaxTexOffset);
                if (parallax != null) tempMat.SetFloat("_Parallax", (float)parallax);
            }

            if (hasNormal)
            {
                if (bumpScale != null)
                {
                    tempMat.SetFloat("_NormalStrength", (float)bumpScale);
                }
                tempMat.SetTexture("_BumpMap", bumpTex);
                tempMat.SetTextureScale("_BumpMap", bumpTexScale);
                tempMat.SetTextureOffset("_BumpMap", bumpTexOffset);

                tempMat.SetTexture("_NormalTex", bumpTex);
                tempMat.SetTextureScale("_NormalTex", bumpTexScale);
                tempMat.SetTextureOffset("_NormalTex", bumpTexOffset);
            }

            if (emissionColor != null)
            {
                tempMat.SetColor("_EmColor", (Color)emissionColor);
                tempMat.SetFloat("_EmPower", 1);
            }

            //set this keyword in unity if you want your model to show backfaces
            //in unity, right click the inspector tab and choose Debug
            if (tempMat.IsKeywordEnabled("NOCULL"))
            {
                tempMat.SetInt("_Cull", 0);
            }
            //set this keyword in unity if you've set up your model for limb removal item displays (eg. goat hoof) by setting your model's vertex colors
            if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
            {
                tempMat.SetInt("_LimbRemovalOn", 1);
            }

            cachedMaterials.Add(tempMat);
            return tempMat;
        }


        /// <summary>
        /// A complicated helper method that sets up strings entered into it to be formatted similar to Risk of Rain 1's manifest style.
        /// </summary>
        /// <param name="deviceName">Name of the Item or Equipment</param>
        /// <param name="estimatedDelivery">MM/DD/YYYY</param>
        /// <param name="sentTo">Specific Location, Specific Area, General Area. E.g. Neptune's Diner, Albatross City, Neptune.</param>
        /// <param name="trackingNumber">An order number. E.g. 667********</param>
        /// <param name="devicePickupDesc">The short description of the item or equipment.</param>
        /// <param name="shippingMethod">Specific instructions on how to handle it, delimited by /. E.g. Light / Fragile</param>
        /// <param name="orderDetails">The actual lore snippet for the item or equipment.</param>
        /// <returns>A string formatted with all of the above in the style of Risk of Rain 1's manifests.</returns>
        /*public static string OrderManifestLoreFormatter(string deviceName, string estimatedDelivery, string sentTo, string trackingNumber, string devicePickupDesc, string shippingMethod, string orderDetails)
        {
            string[] Manifest =
            {
                $"<align=left>Estimated Delivery:<indent=70%>Sent To:</indent></align>",
                $"<align=left>{estimatedDelivery}<indent=70%>{sentTo}</indent></align>",
                "",
                $"<indent=1%><style=cIsDamage><size=125%><u>  Shipping Details:\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0</u></size></style></indent>",
                "",
                $"<indent=2%>-Order: <style=cIsUtility>{deviceName}</style></indent>",
                $"<indent=4%><style=cStack>Tracking Number:  {trackingNumber}</style></indent>",
                "",
                $"<indent=2%>-Order Description: {devicePickupDesc}</indent>",
                "",
                $"<indent=2%>-Shipping Method: <style=cIsHealth>{shippingMethod}</style></indent>",
                "",
                "",
                "",
                $"<indent=2%>-Order Details: {orderDetails}</indent>",
                "",
                "",
                "",
                "<style=cStack>Delivery being brought to you by the brand new </style><style=cIsUtility>Orbital Drop-Crate System (TM)</style>. <style=cStack><u>No refunds.</u></style>"
            };
            return String.Join("\n", Manifest);
        }*/

        /// <summary>
        /// Refreshes stacks of a timed buff on a body for a specified duration. Will refresh the entire stack pool of the buff at once.
        /// </summary>
        /// <param name="body">The body to check.</param>
        /// <param name="buffDef">The buff to refresh.</param>
        /// <param name="duration">The duration all stacks should have.</param>
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = duration;
                }
            }
        }

        /// <summary>
        /// Refreshes stacks of a timed buff on a body for a specified duration, but spreads their time to decay after a set start duration and interval afterwards.
        /// <para>Will refresh the entire stack pool of the buff at once.</para>
        /// </summary>
        /// <param name="body">The body to check.</param>
        /// <param name="buffDef">The buff to refresh.</param>
        /// <param name="taperStart">How long should we wait before beginning to decay buffs?</param>
        /// <param name="taperDuration">The duration between stack decay.</param>
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float taperStart, float taperDuration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            int i = 0;
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = taperStart + i * taperDuration;
                    i++;
                }
            }
        }

        /// <summary>
        /// Adds a timed buff to a body if a Dot for it does not exist, else inflicts said dot on the specified body.
        /// </summary>
        /// <param name="buff">The buffdef to apply to the body, or find the dotcontroller of.</param>
        /// <param name="duration">The duration of the buff or dot.</param>
        /// <param name="stackCount">The amount of buff stacks to apply.</param>
        /// <param name="body">The body to apply the buff or dot to.</param>
        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            if (!NetworkServer.active) { return; }

            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuff(buff.buffIndex, duration);
                }
            }
        }

        /// <summary>
        /// Finds the associated DotController from a buff, if applicable.
        /// </summary>
        /// <param name="buff">The buff to check all dots against.</param>
        /// <returns>A dotindex of the DotController the target buff is associated with, else, it will return an invalid index.</returns>
        public static DotController.DotIndex FindAssociatedDotForBuff(BuffDef buff)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);

            return index;
        }

        public static float StackingLinear(int itemCount, float baseValue, float stackValue)
        {
            return baseValue + stackValue * (itemCount - 1);
        }

        public static int StackingLinear(int itemCount, int baseValue, int stackValue)
        {
            return baseValue + stackValue * (itemCount - 1);
        }

        public static bool TryGetBuffCount(CharacterBody characterBody, BuffDef buffDef, out int buffCount)
        {
            buffCount = 0;
            if (!characterBody) return false;

            buffCount = characterBody.GetBuffCount(buffDef);
            return buffCount > 0;
        }

        public class OverlayPreviewer : MonoBehaviour
        {
            public bool EnableToPreview = false;
            public string materialAssetPath = "";

            [HideInInspector]
            public Material materialInstance;

            public bool isAssigned;

            public CharacterModel assignedCharacterModel;

            public CharacterModel inspectorCharacterModel;

            public bool animateShaderAlpha;

            public AnimationCurve alphaCurve;

            public enum AnimationCurveType
            {
                Constant,
                Linear,
                EaseInAndOut
            }

            public float constantAnimCurveFloat = 1;

            public AnimationCurveType animationCurveType = AnimationCurveType.Constant;

            public float duration = 2;

            public void Update()
            {
                if (EnableToPreview)
                {
                    EnableToPreview = false;
                    TemporaryOverlay temporaryOverlay = gameObject.GetComponent<ModelLocator>().modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = duration;
                    temporaryOverlay.animateShaderAlpha = animateShaderAlpha;
                    //temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    //temporaryOverlay.alphaCurve = AnimationCurve.Linear(0f, 1f, duration, 1f);
                    temporaryOverlay.alphaCurve = AnimationCurve.Constant(0, duration, constantAnimCurveFloat);
                    //if (alphaCurve != null) temporaryOverlay.alphaCurve = alphaCurve;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    try
                    {
                        temporaryOverlay.originalMaterial = Assets.LoadAddressable<Material>(materialAssetPath);
                        temporaryOverlay.AddToCharacerModel(gameObject.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>());
                    }
                    catch
                    {
                        UnityEngine.Object.Destroy(temporaryOverlay);
                        Chat.AddMessage("Exception on preview");
                    }
                }
            }
        }
    }
}