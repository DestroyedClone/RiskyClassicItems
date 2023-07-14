using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace RiskyClassicItems.Artifact
{
    internal class ExampleArtifact : ArtifactBase<ExampleArtifact>
    {
        public override string ArtifactName => "Artifact of the Print Message";

        public override string ArtifactLangTokenName => "PRINTMESSAGEARTIFACT";

        public override Sprite ArtifactEnabledIcon => LoadSprite(true);

        public override Sprite ArtifactDisabledIcon => LoadSprite(false);

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
        }

        public override void OnArtifactEnabled()
        {
            CharacterMaster.onStartGlobal += PrintMessage;
        }

        public override void OnArtifactDisabled()
        {
            CharacterMaster.onStartGlobal -= PrintMessage;
        }

        private void PrintMessage(CharacterMaster characterMaster)
        {
            Chat.AddMessage("oy");
        }
    }
}