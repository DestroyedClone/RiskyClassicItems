using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskyClassicItems.Artifact
{
    internal class ExampleArtifact : ArtifactBase<ExampleArtifact>
    {
        public override string ArtifactName => "Artifact of the Twine";

        public override string ArtifactLangTokenName => "GEYSERSAREROPE";

        public override Sprite ArtifactEnabledIcon => LoadSprite(true);

        public override Sprite ArtifactDisabledIcon => LoadSprite(false);

        public static GameObject ropeGameObject;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            CreateAssets();
        }

        private void CreateAssets()
        {
            ropeGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ropeGameObject.AddComponent<NetworkIdentity>();

            var collider = ropeGameObject.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;


            var nograv = ropeGameObject.AddComponent<NoGravZone>();

            PrefabAPI.RegisterNetworkPrefab(ropeGameObject);
        }

        public override void OnArtifactEnabled()
        {
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
        }

        public override void OnArtifactDisabled()
        {
            Stage.onStageStartGlobal -= Stage_onStageStartGlobal;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            if (NetworkServer.active)
            {
                var jumpVolumes = UnityEngine.Object.FindObjectsOfType<JumpVolume>();
                foreach (var jumpVolume in jumpVolumes)
                {

                }
            }
        }

        //see NoGravZone
        private class RCI_Rope : MonoBehaviour
        {
            public void OnTriggerEnter(Collider other)
            {
                ICharacterGravityParameterProvider component = other.GetComponent<ICharacterGravityParameterProvider>();
                if (component != null)
                {
                    CharacterGravityParameters gravityParameters = component.gravityParameters;
                    gravityParameters.environmentalAntiGravityGranterCount++;
                    component.gravityParameters = gravityParameters;
                }
                ICharacterFlightParameterProvider component2 = other.GetComponent<ICharacterFlightParameterProvider>();
                if (component2 != null)
                {
                    CharacterFlightParameters flightParameters = component2.flightParameters;
                    flightParameters.channeledFlightGranterCount++;
                    component2.flightParameters = flightParameters;
                }
                SkillLocator skillLocator = other.GetComponent<SkillLocator>();
                if (skillLocator)
                {
                    skillLocator.primary.SetSkillOverride()
                }
            }

            public void OnTriggerExit(Collider other)
            {
                ICharacterFlightParameterProvider component = other.GetComponent<ICharacterFlightParameterProvider>();
                if (component != null)
                {
                    CharacterFlightParameters flightParameters = component.flightParameters;
                    flightParameters.channeledFlightGranterCount--;
                    component.flightParameters = flightParameters;
                }
                ICharacterGravityParameterProvider component2 = other.GetComponent<ICharacterGravityParameterProvider>();
                if (component2 != null)
                {
                    CharacterGravityParameters gravityParameters = component2.gravityParameters;
                    gravityParameters.environmentalAntiGravityGranterCount--;
                    component2.gravityParameters = gravityParameters;
                }
                SkillLocator skillLocator = other.GetComponent<SkillLocator>();
                if (skillLocator)
                {

                }
            }
        }
    }
}