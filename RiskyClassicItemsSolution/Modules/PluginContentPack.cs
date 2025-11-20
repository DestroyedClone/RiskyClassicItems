using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicItemsReturns.Modules
{
    public class PluginContentPack : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => ClassicItemsReturnsPlugin.ModGuid;

        public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
        public static List<ItemDef> itemDefs = new List<ItemDef>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EquipmentDef> equipmentDefs = new List<EquipmentDef>();
        public static List<CraftableDef> craftableDefs = new List<CraftableDef>();
        public static List<ArtifactDef> artifactDefs = new List<ArtifactDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.itemDefs.Add(itemDefs.ToArray());
            contentPack.craftableDefs.Add(craftableDefs.ToArray());
            contentPack.equipmentDefs.Add(equipmentDefs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.artifactDefs.Add(artifactDefs.ToArray());
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.unlockableDefs.Add(unlockableDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
