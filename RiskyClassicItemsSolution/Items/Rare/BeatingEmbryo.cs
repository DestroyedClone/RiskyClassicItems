using BepInEx.Configuration;
using ClassicItemsReturns.Items;
using ClassicItemsReturns.Utils;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UI.Image;

namespace RiskyClassicItems.Items
{
    public class BeatingEmbryo : ItemBase<BeatingEmbryo>
    {
        //Upon using your equipment, 30% chance (+20% chance per stack) to activate your equipment again for 5% less power.
        //Subsequent usages further reudce.
        //Can overchance (120% -> guarantee + 20% roll)
        //310% -> 3 guaranteed activations and a +10% chance.
        public const float chance = 30f;
        public const float chancePerStack = 20f;
        public const float repeatUsageMultiplier = 0.95f;

        public override string ItemName => "Beating Embryo";

        public override string ItemLangTokenName => "BEATINGEMBRYO";

        public override object[] ItemFullDescriptionParams => new object[]
        {
        };

        public override object[] ItemPickupDescParams => new object[]
        {
        };

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => LoadItemModel("USB");

        public override Sprite ItemIcon => LoadItemSprite("USB");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            //RoR2.EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated;
            On.RoR2.EquipmentSlot.FireCommandMissile += EquipmentSlot_FireCommandMissile;
            On.RoR2.EquipmentSlot.FireFruit += EquipmentSlot_FireFruit;
            On.RoR2.EquipmentSlot.FireDroneBackup += EquipmentSlot_FireDroneBackup;
            On.RoR2.EquipmentSlot.FireMeteor += EquipmentSlot_FireMeteor;
            On.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;
            On.RoR2.EquipmentSlot.FireSaw += EquipmentSlot_FireSaw;
            On.RoR2.EquipmentSlot.FireCritOnUse += EquipmentSlot_FireCritOnUse;
            On.RoR2.EquipmentSlot.FireBfg += EquipmentSlot_FireBfg;
            On.RoR2.EquipmentSlot.FireJetpack += EquipmentSlot_FireJetpack;
            On.RoR2.EquipmentSlot.FireLightning += EquipmentSlot_FireLightning;
            On.RoR2.EquipmentSlot.FireBossHunter += EquipmentSlot_FireBossHunter;
            On.RoR2.EquipmentSlot.FireBossHunterConsumed += EquipmentSlot_FireBossHunterConsumed;
            On.RoR2.EquipmentSlot.FirePassiveHealing += EquipmentSlot_FirePassiveHealing;
            On.RoR2.EquipmentSlot.FireBurnNearby += EquipmentSlot_FireBurnNearby;
            On.RoR2.EquipmentSlot.FireScanner += EquipmentSlot_FireScanner;
            On.RoR2.EquipmentSlot.FireCrippleWard += EquipmentSlot_FireCrippleWard;
            On.RoR2.EquipmentSlot.FireTonic += EquipmentSlot_FireTonic;
            On.RoR2.EquipmentSlot.FireCleanse += EquipmentSlot_FireCleanse;
            On.RoR2.EquipmentSlot.FireFireBallDash += EquipmentSlot_FireFireBallDash;
            On.RoR2.EquipmentSlot.FireGainArmor += EquipmentSlot_FireGainArmor;
            //FireRecycle
            //FireGateway
            On.RoR2.EquipmentSlot.FireLifeStealOnHit += EquipmentSlot_FireLifeStealOnHit;
            On.RoR2.EquipmentSlot.FireTeamWarCry += EquipmentSlot_FireTeamWarCry;
            //FireDeathProjectile
            On.RoR2.EquipmentSlot.FireMolotov += EquipmentSlot_FireMolotov;
            //firevending machine....
            //firegummyclone
            //FireHealAndRevive
            //FireSproutOfLife

        }

        private bool EquipmentSlot_FireMolotov(On.RoR2.EquipmentSlot.orig_FireMolotov orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                Ray aimRay = self.GetAimRay();
                GameObject prefab = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MolotovClusterProjectile"));
                prefab.GetComponent<ProjectileImpactExplosion>().childrenCount += p;
                ProjectileManager.instance.FireProjectile(prefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), self.gameObject, self.characterBody.damage * repeatUsageMultiplier, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireTeamWarCry(On.RoR2.EquipmentSlot.orig_FireTeamWarCry orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                //don't, stacking sounds makes it louder.
                //Util.PlaySound("Play_teamWarCry_activate", self.characterBody.gameObject);
                Vector3 corePosition = self.characterBody.corePosition;
                EffectData effectData = new EffectData
                {
                    origin = corePosition
                };
                effectData.SetNetworkedObjectReference(self.characterBody.gameObject);

                var buffDuration = 7f;
                var finalBuffDuration = buffDuration;
                for (int i = 0; i < p; i++)
                {
                    buffDuration *= repeatUsageMultiplier;
                    finalBuffDuration += buffDuration;
                    effectData.scale *= repeatUsageMultiplier;
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeamWarCryActivation"), effectData, true);
                }
                self.characterBody.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, finalBuffDuration);
                TeamComponent[] array = UnityEngine.Object.FindObjectsOfType<TeamComponent>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].teamIndex == self.teamComponent.teamIndex)
                    {
                        array[i].GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.TeamWarCry, finalBuffDuration);
                    }
                }
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireLifeStealOnHit(On.RoR2.EquipmentSlot.orig_FireLifeStealOnHit orig, EquipmentSlot self)
        {
            var originalDuration = 8f;
            var moddedDuration = originalDuration;
            var finalDuration = originalDuration;
            EffectData effectData = new EffectData
            {
                origin = self.characterBody.corePosition
            };
            effectData.SetHurtBoxReference(self.characterBody.gameObject);
            if (EmbryoProc(self, out int p))
            {
                for (int i = 0; i < p; i++)
                {
                    moddedDuration *= repeatUsageMultiplier;
                    finalDuration += moddedDuration;
                    effectData.scale *= repeatUsageMultiplier;
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LifeStealOnHitActivation"), effectData, false);
                }
                self.characterBody.AddTimedBuff(RoR2Content.Buffs.LifeSteal, originalDuration + finalDuration);
            }
            return orig(self);
        }



        //firecritonuse basically
        private bool EquipmentSlot_FireGainArmor(On.RoR2.EquipmentSlot.orig_FireGainArmor orig, EquipmentSlot self)
        {
            var originalDuration = 5f;
            var moddedDuration = originalDuration;
            var finalDuration = originalDuration;
            if (EmbryoProc(self, out int p))
            {
                for (int i = 0; i < p; i++)
                {
                    moddedDuration *= repeatUsageMultiplier;
                    finalDuration += moddedDuration;
                }
                self.characterBody.AddTimedBuff(RoR2Content.Buffs.ElephantArmorBoost, originalDuration + finalDuration);
            }
            return orig(self);
        }

        //longer duration maybe?
        private bool EquipmentSlot_FireFireBallDash(On.RoR2.EquipmentSlot.orig_FireFireBallDash orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        //wtf do i do for this
        private bool EquipmentSlot_FireCleanse(On.RoR2.EquipmentSlot.orig_FireCleanse orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireTonic(On.RoR2.EquipmentSlot.orig_FireTonic orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                var buffDuration = EquipmentSlot.tonicBuffDuration;
                var badChance = 20f;
                for (int i = 0; i < p; i++)
                {
                    buffDuration *= repeatUsageMultiplier;
                    badChance *= repeatUsageMultiplier;
                    //inverting the if for math
                    // if not 80% chance is harder than if 20% chance
                    if (Util.CheckRoll(badChance, self.characterBody.master))
                    {
                        self.characterBody.pendingTonicAfflictionCount++;
                    }
                }
                self.characterBody.AddTimedBuff(RoR2Content.Buffs.TonicBuff, EquipmentSlot.tonicBuffDuration);
            }
            return orig(self);
        }

        //spawn multiple?
        private bool EquipmentSlot_FireCrippleWard(On.RoR2.EquipmentSlot.orig_FireCrippleWard orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        //IL intercept prefab spawn, set scanner duration higher instead of multiple pulses.
        private bool EquipmentSlot_FireScanner(On.RoR2.EquipmentSlot.orig_FireScanner orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireBurnNearby(On.RoR2.EquipmentSlot.orig_FireBurnNearby orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                var duration = 12f;
                for (int i = 0; i < p; i++)
                {
                    duration *= repeatUsageMultiplier;
                    self.characterBody.AddHelfireDuration(duration);
                }
            }
            return orig(self);
        }

        private bool EquipmentSlot_FirePassiveHealing(On.RoR2.EquipmentSlot.orig_FirePassiveHealing orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                self.UpdateTargets(RoR2Content.Equipment.PassiveHealing.equipmentIndex, true);
                GameObject rootObject = self.currentTarget.rootObject;
                CharacterBody characterBody = (rootObject?.GetComponent<CharacterBody>()) ?? self.characterBody;
                if (characterBody)
                {
                    HealthComponent healthComponent = characterBody.healthComponent;
                    var healAmount = 0.1f;
                    for (int i = 0; i < p; i++)
                    {
                        healAmount *= repeatUsageMultiplier;
                        EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/WoodSpriteHeal"), characterBody.corePosition, Vector3.up, true);
                        healthComponent?.HealFraction(healAmount, default);
                    }
                }
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireBossHunterConsumed(On.RoR2.EquipmentSlot.orig_FireBossHunterConsumed orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                for (int i = 0; i < p; i++)
                {
                    Chat.SendBroadcastChat(new Chat.BodyChatMessage
                    {
                        bodyObject = self.characterBody.gameObject,
                        token = "EQUIPMENT_BOSSHUNTERCONSUMED_CHAT"
                    });
                }
                self.subcooldownTimer = 1f;
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireBossHunter(On.RoR2.EquipmentSlot.orig_FireBossHunter orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireLightning(On.RoR2.EquipmentSlot.orig_FireLightning orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                self.UpdateTargets(RoR2Content.Equipment.Lightning.equipmentIndex, true);
                HurtBox hurtBox = self.currentTarget.hurtBox;
                if (hurtBox)
                {
                    LightningStrikeOrb orb = new LightningStrikeOrb
                    {
                        attacker = self.gameObject,
                        damageColorIndex = DamageColorIndex.Item,
                        procChainMask = default,
                        procCoefficient = 1f,
                        target = hurtBox,
                        damageValue = self.characterBody.damage * 30f
                    };

                    for (int i = 0; i < p; i++)
                    {
                        orb.damageValue *= repeatUsageMultiplier;
                        orb.isCrit = Util.CheckRoll(self.characterBody.crit, self.characterBody.master);
                        orb.procCoefficient *= repeatUsageMultiplier;
                        OrbManager.instance.AddOrb(orb);
                    }
                    return true;
                }
                return false;
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireJetpack(On.RoR2.EquipmentSlot.orig_FireJetpack orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireBfg(On.RoR2.EquipmentSlot.orig_FireBfg orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireCritOnUse(On.RoR2.EquipmentSlot.orig_FireCritOnUse orig, EquipmentSlot self)
        {
            var originalDuration = 8f;
            var moddedDuration = originalDuration;
            var finalDuration = originalDuration;
            if (EmbryoProc(self, out int p))
            {
                for (int i = 0; i < p; i++)
                {
                    moddedDuration *= repeatUsageMultiplier;
                    finalDuration += moddedDuration;
                }
                self.characterBody.AddTimedBuff(RoR2Content.Buffs.FullCrit, originalDuration + finalDuration);
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireSaw(On.RoR2.EquipmentSlot.orig_FireSaw orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
            }
            return orig(self);
        }

        private void FireSingleSaw(CharacterBody firingCharacterBody, Vector3 origin, Quaternion rotation)
		{
            GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/Sawmerang");
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                crit = firingCharacterBody.RollCrit(),
                damage = firingCharacterBody.damage,
                damageColorIndex = DamageColorIndex.Item,
                force = 0f,
                owner = firingCharacterBody.gameObject,
                position = origin,
                rotation = rotation
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

    private bool EquipmentSlot_FireBlackhole(On.RoR2.EquipmentSlot.orig_FireBlackhole orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                Vector3 position = self.transform.position;
                Ray aimRay = self.GetAimRay();
                GameObject prefab = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere"));
                //something for gravsphere
                ProjectileManager.instance.FireProjectile(prefab, position, Util.QuaternionSafeLookRotation(aimRay.direction), self.gameObject, 0f, 0f, false, DamageColorIndex.Default, null, -1f);
                return true;
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireMeteor(On.RoR2.EquipmentSlot.orig_FireMeteor orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int procCount))
            {
                MeteorStormController component = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm"), self.characterBody.corePosition, Quaternion.identity).GetComponent<MeteorStormController>();
                component.owner = self.gameObject;
                component.ownerDamage = self.characterBody.damage;
                for (int i = 0 ; i < procCount; i++)
                {
                    component.waveCount = Mathf.CeilToInt(component.waveCount * repeatUsageMultiplier);
                    component.ownerDamage *= repeatUsageMultiplier;
                    component.isCrit = Util.CheckRoll(self.characterBody.crit, self.characterBody.master);
                    NetworkServer.Spawn(component.gameObject);
                }
            }
            return orig(self);
        }

        private bool EquipmentSlot_FireDroneBackup(On.RoR2.EquipmentSlot.orig_FireDroneBackup orig, EquipmentSlot self)
        {
            //Requires IL Hook
            return orig(self);
        }

        //Activates 
        private bool EquipmentSlot_FireFruit(On.RoR2.EquipmentSlot.orig_FireFruit orig, EquipmentSlot self)
        {
            if (self.healthComponent && EmbryoProc(self, out int procCount))
            {
                EffectData effectData = new EffectData();
                effectData.origin = self.transform.position;
                effectData.SetNetworkedObjectReference(self.gameObject);
                var healAmount = 0.5f;
                for (int i = 0; i < procCount; i++)
                {
                    effectData.scale *= repeatUsageMultiplier;
                    healAmount *= repeatUsageMultiplier;
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FruitHealEffect"), effectData, true);
                    self.healthComponent.HealFraction(healAmount, default);
                }
            }
            return orig(self);
        }

        public static bool EmbryoProc(EquipmentSlot equipmentSlot, out int procCount)
        {
            return EmbryoProc(equipmentSlot.characterBody, out procCount);
        }

        public static bool EmbryoProc(CharacterBody characterBody, out int procCount)
        {
            procCount = 0;
            if (!characterBody) return false;
            if (!characterBody.master) return false;
            var itemCount = BeatingEmbryo.Instance.GetCount(characterBody);
            if (itemCount == 0) return false;
            var calcChance = ItemHelpers.StackingLinear(itemCount, chance, chancePerStack);

            int guaranteedActivations = (int)System.Math.Truncate(calcChance / 100);
            float resultingChance = calcChance % 100;

            procCount = guaranteedActivations + (RoR2.Util.CheckRoll(resultingChance, characterBody.master) ? 1 : 0);
            return procCount > 0;
        }

        //Fires 12 / 2 = 6 missiles
        //
        private bool EquipmentSlot_FireCommandMissile(On.RoR2.EquipmentSlot.orig_FireCommandMissile orig, EquipmentSlot self)
        {
            if (EmbryoProc(self, out int p))
            {
                //I forgot the formula
                var rocketCount = 12;
                for (int i = 0; i < p; i++)
                {
                    rocketCount = (int)(rocketCount * repeatUsageMultiplier);
                    self.remainingMissiles += (int)rocketCount;
                }
            }
            return orig(self);
        }

        private void EquipmentSlot_onServerEquipmentActivated(EquipmentSlot slot, EquipmentIndex index)
        {

        }
    }
}