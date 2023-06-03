using BepInEx.Configuration;
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

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            CreateAssets();
        }

        private void CreateAssets()
        {

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
    }
}