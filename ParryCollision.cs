using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace nailparryeverything;

public class ParryCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //! HEALTH MANAGER
        var potentialHealthManager = other.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            var fsm = PlayMakerFSM.FindFsmOnGameObject(potentialHealthManager.gameObject, "Control");
            if (fsm == null) fsm = potentialHealthManager.gameObject.GetComponent<PlayMakerFSM>();
            if (PlayerData.instance.silk >= 9)
            {
                var parryHit = new HitInstance
                {
                    Source = HeroController.instance.gameObject,
                    AttackType = AttackTypes.Nail,
                    DamageDealt = (int)(PlayerData.instance.nailDamage * PlayerData.instance.silk * nailparryeverythingPlugin.PARRY_DAMAGE_MULTIPLIER),
                    Direction = 0f,
                    Multiplier = 1f,
                    MagnitudeMultiplier = 1f,
                    IgnoreInvulnerable = false,
                    HitEffectsType = EnemyHitEffectsProfile.EffectsTypes.Full,
                    IsNailTag = true
                };
                nailparryeverythingPlugin.SetHealthManagerInvincibility(potentialHealthManager, false);
                potentialHealthManager.TakeDamage(parryHit);
                HeroController.instance.TakeSilk(PlayerData.instance.silk);
                GameManager.instance.FreezeMoment(FreezeMomentTypes.BossStun);
            } else nailparryeverythingPlugin.SetHealthManagerInvincibility(potentialHealthManager, true);
            logfsmdata(fsm);
            //? If the enemy related to this HealthManager isnt in a parryable active state, stop the function
            if (!tweaks.IsValidActiveState(fsm.name, fsm.ActiveStateName)) return;
        }
        //! DAMAGE HERO
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if ((damageHero == null || !damageHero.enabled || tweaks.CheckBlacklist(other)) && !tweaks.CheckWhitelist(other)) return;
        if (damageHero != null) nailparryeverythingPlugin.Instance.StartCoroutine(damageHero.NailClash(0, "NPE PARRY", transform.position));
        HeroController.instance.NailParry();
        GameManager.instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
        if (damageHero != null) StartCoroutine(DisableDamageHero(damageHero));
        else
        {
            HeroController.instance.StartInvulnerable(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
            nailparryeverythingPlugin.OnParry();
        }
    }

    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
        //TODO: IF THE PARENT OBJECT GETS DESTROYED AND WE REENABLE THIS IT BREAKS, CHECK WITH PUTRIFIED DUCT SQUITS
        __instance.enabled = true;
    }

    private static void logfsmdata(PlayMakerFSM fsm)
    {
        nailparryeverythingPlugin.log(@"/// [NAIL PARRY EVERYTHING NEEDLE HIT] \\\");
        nailparryeverythingPlugin.log("INTERNAL ENEMY NAME: \"" + fsm.name + "\"");
        nailparryeverythingPlugin.log("ACTIVE STATE NAME:   \"" + fsm.ActiveStateName + "\"");
        nailparryeverythingPlugin.log("AI TYPE:             \"" + fsm.Fsm.name + "\"");
        nailparryeverythingPlugin.log("WHITELIST FORMAT:    \"" + $"{{ \"{fsm.name}\", [\"{fsm.ActiveStateName}\"] }}," + "\"");
        nailparryeverythingPlugin.log(@"\\\ [NAIL PARRY EVERYTHING NEEDLE HIT] ///" + "\n");
    }
}