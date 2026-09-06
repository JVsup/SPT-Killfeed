using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using SPT.Killfeed.Models;
using SPT.Killfeed.Utilities;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Killfeed.Patches;

public class ActiveHealthApplyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActiveHealthController).GetMethod(
            nameof(ActiveHealthController.ApplyDamage),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(EBodyPart), typeof(float), typeof(DamageInfo) },
            null);
    }

    private static string ResolveWeaponLabel(Item weaponItem)
    {
        if (weaponItem is Weapon weapon)
        {
            var factory = Singleton<ItemFactory>.Instance;
            return factory.BriefItemName(weapon, weapon.ShortName.Localized());
        }

        return string.Empty;
    }

    private static string ResolveAmmoName(string ammoTemplateId)
    {
        if (string.IsNullOrEmpty(ammoTemplateId))
        {
            return string.Empty;
        }

        var templates = Singleton<ItemFactory>.Instance.ItemTemplates;
        if (templates.TryGetValue((MongoID)ammoTemplateId, out var template))
        {
            return template.ShortNameLocalizationKey.Localized();
        }

        return ammoTemplateId;
    }

    [PatchPostfix]
    private static void Postfix(
        ActiveHealthController __instance,
        EBodyPart __0,
        float __1,
        DamageInfo __2)
    {
        var bodyPart = __0;
        var damageArgument = __1;
        var damageInfo = __2;

        var victim = __instance.Player;
        if (victim == null)
        {
            return;
        }

        var attacker = damageInfo.Player != null
            ? damageInfo.Player.iPlayer as Player
            : null;

        var bodyDamage = Mathf.Max(0f, damageInfo.DidBodyDamage);
        var armorDamage = Mathf.Max(0f, damageInfo.DidArmorDamage);
        if (bodyDamage <= 0.0001f && damageArgument > 0f)
        {
            bodyDamage = damageArgument;
        }

        var blocked = damageInfo.BlockedBy.HasValue;
        var deflected = damageInfo.DeflectedBy.HasValue;
        var armorOnly = armorDamage > 0.01f && bodyDamage <= 0.01f || blocked || deflected;

        var weaponLabel = damageInfo.Weapon != null
            ? ResolveWeaponLabel(damageInfo.Weapon)
            : string.Empty;
        var ammoName = ResolveAmmoName(damageInfo.SourceId);

        var distance = 0f;
        try
        {
            var hitPoint = damageInfo.HitPoint;
            if (attacker != null && hitPoint != default)
            {
                distance = Vector3.Distance(attacker.Transform.position, hitPoint);
            }
            else if (attacker != null)
            {
                distance = Vector3.Distance(attacker.Transform.position, victim.Transform.position);
            }
        }
        catch
        {
            distance = 0f;
        }

        var damageEvent = new DamageEvent
        {
            AttackerId = attacker?.Profile?.Id,
            AttackerName = attacker?.Profile?.Nickname ?? "Unknown",
            AttackerSide = attacker != null ? attacker.Side : EPlayerSide.Savage,
            VictimId = victim.Profile?.Id,
            VictimName = victim.Profile?.Nickname ?? "Unknown",
            BodyPart = bodyPart.ToString(),
            DamageAmount = damageArgument,
            BodyDamage = bodyDamage,
            ArmorDamage = armorDamage,
            IsLocalAttacker = attacker != null && ReferenceEquals(attacker, State.LocalPlayer),
            IsLocalVictim = ReferenceEquals(victim, State.LocalPlayer),
            IsHeadshot = bodyPart == EBodyPart.Head,
            VictimIsDead = victim.HealthController != null && !victim.HealthController.IsAlive,
            WorldPos = damageInfo.HitPoint,
            WeaponLabel = weaponLabel,
            AmmoName = ammoName,
            IsArmorHit = armorOnly,
            Ricochet = deflected,
            Blocked = blocked,
            DistanceMeters = distance
        };

        if (damageEvent.VictimIsDead)
        {
            EventBus.RaiseKill(damageEvent);
        }
    }
}

