using ClassicItemsReturns.Artifact;
using UnityEngine;
using System.Collections.Generic;
using RoR2;
using BepInEx.Configuration;
using ClassicItemsReturns;
using UnityEngine.Networking;

namespace ClassicItemsReturns.Artifact
{
    public class ArtifactOfClover : ArtifactBase
    {
        public static ItemDef cloverDef;    //gets set when clover is initialized
        public static List<Inventory> affectedInventories = new List<Inventory>();

        public override string ArtifactName => "Artifact of Clovers";

        public override string ArtifactLangTokenName => "CLOVER";

        public override Sprite ArtifactEnabledIcon => LoadSprite("texArtifactCloverEnabled");

        public override Sprite ArtifactDisabledIcon => LoadSprite("texArtifactCloverDisabled");

        public override void Init(ConfigFile config)
        {
            bool foundItem = false;
            if (cloverDef != null)
            {
                //Items only get added to this list if they are enabled.
                foreach (Items.ItemBase item in ClassicItemsReturnsPlugin.Items)
                {
                    if (item.ItemDef == cloverDef)
                    {
                        foundItem = true;
                        break;
                    }
                }
            }
            if (!foundItem)
            {
                Debug.LogError("Artifact of Clovers - Could not find 56 Leaf Clover. Artifact will not be initialized.");
                return;
            }
            base.Init(config);
        }

        public override void Hooks()
        {
            RoR2.Run.onRunStartGlobal += ResetInventoryList;
        }

        public override void OnArtifactEnabled()
        {
            RoR2.CharacterBody.onBodyStartGlobal += GiveCloverOnSpawn;
        }

        public override void OnArtifactDisabled()
        {
            RoR2.CharacterBody.onBodyStartGlobal -= GiveCloverOnSpawn;
        }

        private void ResetInventoryList(Run runObject)
        {
            affectedInventories.Clear();
        }

        private void GiveCloverOnSpawn(CharacterBody body)
        {
            if (!NetworkServer.active || !body.isPlayerControlled || !body.inventory) return;
            if (!affectedInventories.Contains(body.inventory))
            {
                affectedInventories.Add(body.inventory);
                body.inventory.GiveItemPermanent(cloverDef);
            }
        }
    }
}
