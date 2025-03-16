using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.GameContent;


namespace necessetest
{
    public class CrystalSentinelPlayer : ModPlayer
    {
        public bool isActive;
        private int cooldown = 0;

        public override void ResetEffects() => isActive = false;

        public override void PostUpdate()
        {
            if (cooldown > 0) cooldown--;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!isActive || cooldown > 0 || Player.dead) return;
            
            Vector2 spawnPos = Player.Center;
            Vector2 velocity = Main.rand.NextVector2CircularEdge(10f, 10f);
            // 计算召唤伤害
            //int damage = (int)Player.GetDamage(DamageClass.Summon).ApplyTo(120);
            //float knockback = 3f;

            // 生成哨兵
            Projectile.NewProjectile(
                Player.GetSource_OnHurt(info.DamageSource),
                spawnPos,
                velocity,
                ProjectileID.RainbowRodBullet,
                150,
                5f,
                Player.whoAmI,
                ai0: 180 // 持续时间帧数
            );

            // 冷却时间0.1秒（180帧）
            cooldown = 6;
        }
    }

    public class CritDamagePlayer : ModPlayer
    {
        public bool cancrit;
        public override void ResetEffects() => cancrit = false;

        // 使用GlobalItem中的攻击NPC
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
    if (!cancrit) return;
        {
         modifiers.CritDamage += 0.25f;
        }
      }
    }

    public class LastChancePlayer : ModPlayer
    {
        public bool hasAmulet;
        public int cooldownTimer;
        private bool canTrigger = true;

        public override void ResetEffects()
        {
            hasAmulet = false;
        }

        public override void PostUpdate()
        {
            if (cooldownTimer > 0)
                cooldownTimer--;
            else
                canTrigger = true;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // 条件检查：未佩戴护符/冷却中 → 不触发
            if (!hasAmulet || !canTrigger)
                return true;

            // 恢复生命25%
            int healAmount = (int)(Player.statLifeMax2 * 0.25f);
            Player.statLife += healAmount;

            // 取消死亡
            playSound = false;
            genGore = false;

            // 特效
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDustPerfect(
                    Player.Center,
                    DustID.LifeDrain,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Scale: 2f
                );
            }

            // 音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.Center);

            // 重置冷却
            cooldownTimer = 18000; // 5分钟 = 60秒 * 60帧/秒 * 5
            canTrigger = false;

            return false;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
        // 冷却时间显示逻辑
        if (cooldownTimer > 0)
          {
            int remainingSeconds = cooldownTimer / 60;
            string cooldownText = $"{remainingSeconds}";
            Vector2 position = Player.Center - Main.screenPosition - new Vector2(0, 50);

            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                cooldownText,
                position.X,
                position.Y,
                Color.Red,
                Color.White,
                Vector2.Zero,
                1f
            );
          }
        }
    }

    public class VampireCloakPlayer : ModPlayer
    {
        public bool hasCloak;
        private int healCooldown;

        public override void ResetEffects() => hasCloak = false;
        public override void PostUpdate() => healCooldown--;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasCloak || healCooldown > 0) return;
            
            int damage = 5;
            SpawnHealProjectile(target, damage);
            healCooldown = 30;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasCloak || healCooldown > 0 || proj.DamageType != DamageClass.Generic) return;

            int damage = 4;
            SpawnHealProjectile(target, damage);
            healCooldown = 30;
        }

        // 生成治疗射弹（原版吸血鬼刀同款逻辑）
        private void SpawnHealProjectile(NPC target, int damage)
        {
            int healAmount = (int)(damage * 1f);
            if (healAmount < 1) healAmount = 1;

            Projectile.NewProjectile(
                Player.GetSource_OnHit(target),
                target.Center,
                Vector2.Zero,
                ProjectileID.VampireHeal,
                100,
                0f,
                Player.whoAmI,
                ai1:healAmount
            );

            // 特效和音效
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(
                    target.Center,
                    DustID.LifeDrain,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Scale: 1.5f
                );
            }
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item17, Player.Center);
        }
    }

    public class MoltenRingPlayer : ModPlayer
    {
        public bool hasMoltenRing;

        public override void ResetEffects() => hasMoltenRing = false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasMoltenRing) return;
                target.AddBuff(BuffID.OnFire3, 300); // 地狱火debuff 5秒
                // 地狱火特效
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfect(
                target.Center,
                DustID.Torch,
                Main.rand.NextVector2Circular(3f, 3f),
                Scale: 1.5f
            );
        }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasMoltenRing) return;
                target.AddBuff(BuffID.OnFire3, 300);
                // 地狱火特效
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfect(
                target.Center,
                DustID.Torch,
                Main.rand.NextVector2Circular(3f, 3f),
                Scale: 1.5f
            );
        }
        }
    }

    public class FrostPendantPlayer : ModPlayer
    {
        public bool hasFrostPendant;

        public override void ResetEffects() => hasFrostPendant = false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasFrostPendant) return;
                target.AddBuff(BuffID.Frostburn, 300); // 霜火debuff 5秒
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfect(
                target.Center,
                DustID.Frost,
                Main.rand.NextVector2Circular(3f, 3f),
                Scale: 1.5f
            );
        }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasFrostPendant) return;
                target.AddBuff(BuffID.Frostburn, 300);
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfect(
                target.Center,
                DustID.Frost,
                Main.rand.NextVector2Circular(3f, 3f),
                Scale: 1.5f
            );
        }
        }
    }

    public class SpiderCharmBuff : ModPlayer
    {
        public bool hasSpiderCharmBuff;

        public override void ResetEffects() => hasSpiderCharmBuff = false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasSpiderCharmBuff) return;
                target.AddBuff(BuffID.Venom, 300); // 剧毒debuff 5秒
        for (int i = 0; i < 10; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
            Dust.NewDustPerfect(
                target.Center,
                DustID.PurpleMoss,
                speed,
                Alpha: 100,
                Scale: Main.rand.NextFloat(0.8f, 1.5f)
            );
        }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasSpiderCharmBuff) return;
                target.AddBuff(BuffID.Venom, 300);
        for (int i = 0; i < 10; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
            Dust.NewDustPerfect(
                target.Center,
                DustID.PurpleMoss,
                speed,
                Alpha: 100,
                Scale: Main.rand.NextFloat(0.8f, 1.5f)
            );
        }
        }
    }

    public class PolarClawBuff : ModPlayer
    {
        public bool hasPolarClawBuff;

        public override void ResetEffects() => hasPolarClawBuff = false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasPolarClawBuff) return;
                target.AddBuff(BuffID.Webbed, 300);
                target.AddBuff(BuffID.Ichor, 300); // 减速和破甲debuff 5秒
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasPolarClawBuff) return;
                target.AddBuff(BuffID.Webbed, 300);
                target.AddBuff(BuffID.Ichor, 300);
        }
    }
    public class FlightExtensionPlayer : ModPlayer
    {
        public bool flightExtensionActive;

        public override void ResetEffects()
        {
            flightExtensionActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (flightExtensionActive)
            {
                // 延长飞行时间
                Player.wingTimeMax = (int)(Player.wingTimeMax * 1.5f); // 延长50%
                //Player.wingTime = Player.wingTimeMax;  重置当前飞行时间
            }
        }
    }

    public class SummonDebuffPlayer : ModPlayer
    {
        public bool enableSummonDebuff;

        public override void ResetEffects()
        {
            enableSummonDebuff = false;
        }
    }

    public class SummonDebuffGlobalProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 检测是否为召唤物弹射物且玩家佩戴饰品
            if (projectile.DamageType == DamageClass.Summon && 
                Main.player[projectile.owner].GetModPlayer<SummonDebuffPlayer>().enableSummonDebuff)
            {
                // 施加诅咒地狱（持续5秒）和暗影烈焰（持续2秒）
                target.AddBuff(BuffID.CursedInferno, 120);
                target.AddBuff(BuffID.ShadowFlame, 120);
            }
        }
    }

    public class RetaliationPoisonPlayer : ModPlayer
    {
        public bool enableRetaliationPoison;

        public override void ResetEffects()
        {
            enableRetaliationPoison = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (enableRetaliationPoison)
            {
                // 定义中毒范围（半径500像素）
                float radius = 500f;

                // 遍历附近所有NPC
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.Distance(Player.Center) <= radius)
                    {
                        // 施加普通中毒（持续5秒）
                        npc.AddBuff(BuffID.Poisoned, 300);
                    }
                }
            }
        }
    }

    public class ShieldPlayer : ModPlayer
    {
        public bool enableShield; // 是否启用护盾
        public bool shieldActive; // 护盾当前是否激活
        public int shieldCooldown; // 护盾冷却时间（帧）

        public override void ResetEffects()
        {
            enableShield = false;
        }

        public override void PostUpdateEquips()
        {
            if (enableShield)
            {
                // 护盾冷却倒计时
                if (shieldCooldown > 0)
                {
                    shieldCooldown--;
                }
                else if (!shieldActive)
                {
                    // 冷却结束后重新激活护盾
                    shieldActive = true;
                }
            }
            else
            {
                // 未佩戴饰品时重置护盾状态
                shieldActive = false;
                shieldCooldown = 0;
            }

            if (enableShield && shieldActive)
    {
        // 生成护盾粒子
        for (int i = 0; i < 3; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Player.position,
                Player.width,
                Player.height,
                DustID.Electric,
                Scale: 0.25f
            );
            dust.noGravity = true;
            dust.velocity = Player.velocity * 0.1f;
        }
    }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (enableShield && shieldActive)
            {
                // 护盾激活时提供50%减伤
                modifiers.FinalDamage *= 0.7f;

                // 护盾消失并进入冷却
                shieldActive = false;
                shieldCooldown = 30 * 60; // 30秒冷却（30秒 × 60帧/秒）
            }
        }
    }

    public class DamageToManaPlayer : ModPlayer
    {
        public bool enableDamageToMana; // 是否启用效果
        public float damageToManaRatio = 0.5f; // 受伤量转换为蓝量的比例（50%）

        public override void ResetEffects()
        {
            enableDamageToMana = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (enableDamageToMana)
            {
                // 计算转换的蓝量
                int manaGain = (int)(info.Damage * damageToManaRatio);

                // 确保蓝量不超过上限
                manaGain = (int)MathHelper.Min(manaGain, Player.statManaMax2 - Player.statMana);

                // 增加蓝量
                Player.statMana += manaGain;

                // 播放蓝量恢复效果
                Player.ManaEffect(manaGain);
            }
        }
    }
}

namespace necessetest.Items.Accessories {
public class TrackerBoot : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 2;      // 稀有度（蓝色）
            Item.value = Item.sellPrice(0, 0, 2, 0); // 价值2银币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.moveSpeed += 0.1f; // 增加玩家最大速度10%
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Leather, 10); // 10个皮革
            recipe.AddIngredient(ItemID.IronBar, 2); // 2个铁锭
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class BoneHilt : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.GetArmorPenetration(DamageClass.Generic) += 20;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Bone, 40);         // 40个骨头
            recipe.AddIngredient(ItemID.MeteoriteBar, 4);  // 4个陨铁锭
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ForbiddenSpellbook : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 5, 0, 0); // 价值5金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.GetDamage(DamageClass.Magic) += 0.5f;
            player.manaCost += 1f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Book, 10);         // 10本书
            recipe.AddIngredient(ItemID.Ectoplasm, 25);  // 25个灵气
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FuzzyDice : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 1;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 0, 80); // 价值80铜
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.GetCritChance(DamageClass.Generic) += 5f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 20);         // 20丝绸
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class NobleHorseshoe : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 1;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 0, 80); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.GetCritChance(DamageClass.Generic) += 5f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.LeadBar, 20);         // 20个铅锭
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class Carapaceshield : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 15, 0, 0); // 价值15金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.statDefense += 12;
            player.endurance += 0.1f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BeetleHusk, 15);         // 15个甲虫外壳
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FoolsGambit : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 5;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 50, 0, 0); // 价值50金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.statDefense += 10;
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.GetCritChance(DamageClass.Generic) += 10f;
            player.maxMinions += 1; 
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DestroyerEmblem, 1);         // 毁灭者徽章
            recipe.AddIngredient(ItemID.PapyrusScarab, 1);         // 甲虫莎草纸
            recipe.AddIngredient(ItemID.LunarBar, 20);         // 20个夜明锭
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class BloodstoneRing : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 3, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) 
        {
            player.statDefense -= 20;

            float lifeRatio = 1f - (float)player.statLife / player.statLifeMax2;

            if (lifeRatio > 0.5f)
            {
                player.lifeRegen += 20;
            }
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DiamondRing, 1);         // 钻石戒指
            recipe.AddIngredient(ItemID.CrimtaneBar, 5);         // 5个猩红锭
            recipe.AddTile(TileID.TinkerersWorkbench);      // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class LeatherGlove : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 1;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 0, 80); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Leather, 10); // 10个皮革
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ShineBelt : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 2;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 5, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.magicLantern = true; 
            Lighting.AddLight(
                (int)(player.Center.X / 16), 
                (int)(player.Center.Y / 16), 
                1.2f, 1.2f, 1.0f // 更亮的白光
            );
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CopperBar, 5); // 5个铜锭
            recipe.AddIngredient(ItemID.GoldBar, 3); // 3个金锭
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ExplorerSatchel : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
            player.moveSpeed += 0.1f; // 增加玩家最大速度10%
            player.magicLantern = true; 
            Lighting.AddLight(
                (int)(player.Center.X / 16), 
                (int)(player.Center.Y / 16), 
                1.2f, 1.2f, 1.0f // 更亮的白光
            );
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ShineBelt>(), 1); // 闪耀腰带
            recipe.AddIngredient(ModContent.ItemType<LeatherGlove>(), 1); // 皮革手套
            recipe.AddIngredient(ModContent.ItemType<TrackerBoot>(), 1); // 追踪靴
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class SpareGemstones : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 10, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statManaMax2 += 40;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FrostCore, 2); // 2个寒霜核
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class SpellStone : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 5;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 30, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statManaMax2 += 100;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Obsidian, 25); // 25个黑曜石
            recipe.AddIngredient(ModContent.ItemType<SpareGemstones>(), 1); // 冰晶石
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class LuckyCape : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetCritChance(DamageClass.Generic) += 10f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<NobleHorseshoe>(), 1); // 高尚马掌
            recipe.AddIngredient(ModContent.ItemType<FuzzyDice>(), 1); // 幸运骰子
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class RegenPendant : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.lifeRegen += 3;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LifeCrystal, 1); // 生命水晶
            recipe.AddIngredient(ItemID.BandofRegeneration, 1); // 再生手环
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class LifePendant : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.lifeRegen += 6;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LifeFruit, 1); // 生命果
            recipe.AddIngredient(ModContent.ItemType<RegenPendant>(), 1); // 再生吊坠
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class KineticBoots : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 3, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.accRunSpeed += 7f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HermesBoots, 1); // 赫尔墨斯靴
            recipe.AddIngredient(ItemID.HallowedBar, 8); // 8个神圣锭
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class CrystalShield : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 5;
            // 传递激活状态给ModPlayer
            player.GetModPlayer<CrystalSentinelPlayer>().isActive = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrystalShard, 8);  // 水晶碎片
            recipe.AddIngredient(ItemID.HallowedBar, 8);   // 神圣锭
            recipe.AddIngredient(ItemID.SoulofLight, 8); // 光明碎片
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class MagicManual : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.manaRegenBonus *= 2;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Book, 10);         // 10本书
            recipe.AddIngredient(ItemID.CobaltBar, 25);  // 25个钴锭
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class Prophecyslab : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.manaRegenBonus *= 2;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SoulofLight, 10); // 光明之魂
            recipe.AddIngredient(ItemID.HallowHardenedSand, 250);  // 250个珍珠沙
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ScryingCards : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.manaRegenBonus *= 7/2;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MagicManual>(), 1); // 魔法手册
            recipe.AddIngredient(ModContent.ItemType<Prophecyslab>(), 1); // 预言板
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ScryingMirror : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 2;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 9, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.maxTurrets += 1;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Glass, 10); // 10玻璃
            recipe.AddIngredient(ItemID.Wood, 25);  // 25个木头
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class MesmerTablet : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 5, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.maxTurrets += 1;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Ectoplasm, 10);  // 10个灵气
            recipe.AddIngredient(ItemID.SoulofNight, 25);  // 25个黑暗之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class InducingAmulet : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) += 0.3f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FallenStar, 30); // 星星
            recipe.AddIngredient(ItemID.SoulofMight, 15);  // 力量之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class HysteriaTablet : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.maxTurrets += 2;
            player.GetDamage(DamageClass.Summon) += 0.3f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MesmerTablet>(), 1); // 催眠药片
            recipe.AddIngredient(ModContent.ItemType<InducingAmulet>(), 1); // 诱导护身符
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrozenWave : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<CritDamagePlayer>().cancrit = true; // +25%暴伤
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FrostCore, 3); // 3个寒霜核
            recipe.AddIngredient(ItemID.SoulofFright, 15);  // 恐惧之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrenzyOrb : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            float lifeRatio = 1f - (float)player.statLife / player.statLifeMax2;
            
            float damageBoost = MathHelper.Clamp(lifeRatio * 0.4f, 0f, 0.4f);
            player.GetDamage(DamageClass.Generic) += damageBoost;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 12); // 12个狱锭
            recipe.AddIngredient(ItemID.SoulofNight, 20);  // 黑暗之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class ClockworkHeart : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 10, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statLifeMax2 /= 2;
            player.statDefense += player.statLifeMax2/10;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HallowedBar, 20); // 20个神圣锭
            recipe.AddIngredient(ItemID.LifeCrystal, 20);  // 生命水晶
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrozenHeart : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 9, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statLifeMax2 += 50;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PurpleIceBlock, 250); // 250个粉雪块
            recipe.AddIngredient(ItemID.LifeFruit, 10);  // 生命果
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class Lifeline : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<LastChancePlayer>().hasAmulet = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CharmofMyths, 1); // 神话护身符
            recipe.AddIngredient(ItemID.LifeCrystal, 10);  // 生命水晶
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrozenSoul : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 5;      // 稀有度（绿色）
            Item.value = Item.sellPrice(1, 0, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statLifeMax2 += 50;
            player.GetModPlayer<LastChancePlayer>().hasAmulet = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<FrozenHeart>(), 1); // 冰冻之心
            recipe.AddIngredient(ModContent.ItemType<Lifeline>(), 1); // 命悬一线护符
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class VampiresGift : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 5;      // 稀有度（绿色）
            Item.value = Item.sellPrice(1, 0, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.moveSpeed += 0.15f; // 增加玩家最大速度15%
            player.GetModPlayer<VampireCloakPlayer>().hasCloak = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.VampireKnives, 1); // 吸血鬼刀
            recipe.AddIngredient(ItemID.SpectreBar, 10); // 幽魂锭
            recipe.AddIngredient(ItemID.Silk, 50); // 丝绸
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FireStone : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<MoltenRingPlayer>().hasMoltenRing = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 10); // 狱炎锭
            recipe.AddIngredient(ItemID.Obsidian, 10); // 黑曜石
            recipe.AddIngredient(ItemID.Fireblossom, 30); // 火焰花
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrostStone : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<FrostPendantPlayer>().hasFrostPendant = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IceBlock, 500); // 冰块
            recipe.AddIngredient(ItemID.FlowerofFrost, 1); // 寒霜之花
            recipe.AddIngredient(ItemID.Shiverthorn, 30); // 寒颤棘
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

public class FrostFlame : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<MoltenRingPlayer>().hasMoltenRing = true;
            player.GetModPlayer<FrostPendantPlayer>().hasFrostPendant = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<FireStone>(), 1); // 火焰之石
            recipe.AddIngredient(ModContent.ItemType<FrostStone>(), 1); // 极寒之石
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class SpiderCharm : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<SpiderCharmBuff>().hasSpiderCharmBuff = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChlorophyteBar, 12); // 叶绿锭
            recipe.AddIngredient(ItemID.Stinger, 6); // 毒刺
            recipe.AddIngredient(ItemID.JungleSpores, 5); // 丛林孢子
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class PolarClaw : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 8, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<PolarClawBuff>().hasPolarClawBuff = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Bone, 30); // 骨头
            recipe.AddIngredient(ItemID.Shiverthorn, 20); // 寒颤棘
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class SummonFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) += 0.2f;
            player.GetDamage(DamageClass.Ranged) -= 0.2f;
            player.GetDamage(DamageClass.Melee) -= 0.2f;
            player.GetDamage(DamageClass.Magic) -= 0.2f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SummonerEmblem, 1); // 召唤徽章
            recipe.AddIngredient(ItemID.FragmentStardust, 3); // 星云碎片
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class RangeFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) -= 0.2f;
            player.GetDamage(DamageClass.Ranged) += 0.2f;
            player.GetDamage(DamageClass.Melee) -= 0.2f;
            player.GetDamage(DamageClass.Magic) -= 0.2f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.RangerEmblem, 1); // 射手徽章
            recipe.AddIngredient(ItemID.FragmentVortex, 3); // 星旋碎片
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class MeleeFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) -= 0.2f;
            player.GetDamage(DamageClass.Ranged) -= 0.2f;
            player.GetDamage(DamageClass.Melee) += 0.2f;
            player.GetDamage(DamageClass.Magic) -= 0.2f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.WarriorEmblem, 1); // 战士徽章
            recipe.AddIngredient(ItemID.FragmentSolar, 3); // 日耀碎片
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class MagicFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 20, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) -= 0.2f;
            player.GetDamage(DamageClass.Ranged) -= 0.2f;
            player.GetDamage(DamageClass.Melee) -= 0.2f;
            player.GetDamage(DamageClass.Magic) += 0.2f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SorcererEmblem, 1); // 法师徽章
            recipe.AddIngredient(ItemID.FragmentNebula, 3); // 星云碎片
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class BalancedFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 5;      // 稀有度（绿色）
            Item.value = Item.sellPrice(1, 0, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) += 0.15f;
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetDamage(DamageClass.Magic) += 0.15f;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<SummonFoci>(), 1); // 召唤护符
            recipe.AddIngredient(ModContent.ItemType<RangeFoci>(), 1); // 远程护符
            recipe.AddIngredient(ModContent.ItemType<MeleeFoci>(), 1); // 近战护符
            recipe.AddIngredient(ModContent.ItemType<MagicFoci>(), 1); // 魔法护符
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class BalancedFrostfireFoci : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 6;      // 稀有度（绿色）
            Item.value = Item.sellPrice(2, 0, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) += 0.15f;
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetDamage(DamageClass.Magic) += 0.15f;
            player.GetModPlayer<MoltenRingPlayer>().hasMoltenRing = true;
            player.GetModPlayer<FrostPendantPlayer>().hasFrostPendant = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BalancedFoci>(), 1); // 平衡护符
            recipe.AddIngredient(ModContent.ItemType<FrostFlame>(), 1); // 冰火之石
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class Zephyrcharm : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 3;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 6, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<FlightExtensionPlayer>().flightExtensionActive = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SoulofFlight, 10); // 飞行之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }
    
    public class CompanionLocket : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 18, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            //player.GetDamage(DamageClass.Summon) += 0.1f*(player.maxMinions-player.slotsMinions);
            player.GetDamage(DamageClass.Summon) += Math.Max(0, 0.01f*((105-5*(player.maxMinions - player.slotsMinions))*(player.maxMinions - player.slotsMinions)/2));
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PixieDust, 100); // 妖精尘
            recipe.AddIngredient(ItemID.LunarBar, 12);  // 12个夜明锭
            recipe.AddIngredient(ItemID.BlackLens, 2); // 黑色晶状体
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class ChainShirt : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 1;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 0, 0, 90); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {

            float lifeRatio = 1f - (float)player.statLife / player.statLifeMax2;

            if (lifeRatio < 0.1f)
            {
                player.statDefense += 5;
            }
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CopperBar, 14); // 铜锭
            recipe.AddIngredient(ItemID.Rope, 3);  // 3个绳子
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class NecroticSoulSkull : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 22, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetDamage(DamageClass.Summon) += 0.1f;
            player.GetModPlayer<SummonDebuffPlayer>().enableSummonDebuff = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FlaskofPoison, 8); // 毒药瓶
            recipe.AddIngredient(ItemID.SoulofSight, 15); // 视域之魂
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class ShellofRetribution : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 2;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 1, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statDefense += 4;
            player.GetModPlayer<RetaliationPoisonPlayer>().enableRetaliationPoison = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CopperBar, 12); // 铜锭
            recipe.AddIngredient(ItemID.Stinger, 3); // 毒刺
            recipe.AddIngredient(ItemID.JungleSpores, 3); // 丛林孢子
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class GuardianShell : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 24, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<ShieldPlayer>().enableShield = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TurtleShell, 5); // 龟壳
            recipe.AddIngredient(ItemID.ChlorophyteBar, 25); // 叶绿锭
            recipe.AddIngredient(ItemID.JungleSpores, 2); // 丛林孢子
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }

    public class SiphonShield : ModItem {
        public override void SetDefaults() {
            Item.width = 32;   // 物品宽度
            Item.height = 32;  // 物品高度
            Item.accessory = true; // 标记为饰品:cite[1]
            Item.rare = 4;      // 稀有度（绿色）
            Item.value = Item.sellPrice(0, 24, 0, 0); // 价值1金币
        }

        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.statDefense += 6;
            player.endurance += 0.25f;
            player.statManaMax2 += 20;
            player.GetModPlayer<DamageToManaPlayer>().enableDamageToMana = true;
        }

        // 合成配方（可选）
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MagicCuffs, 1); // 魔法手铐
            recipe.AddIngredient(ItemID.PaladinsShield, 1); // 圣骑士盾
            recipe.AddTile(TileID.TinkerersWorkbench);           // 在哥布林工匠台合成
            recipe.Register();
        }
    }
}
