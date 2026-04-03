using System;
using System.Collections;
using System.Linq;
using GlobalEnums;
using UnityEngine;

namespace nailparryeverything;

public class ParryCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if ((damageHero == null || !damageHero.enabled || tweaks.CheckBlacklist(other)) && !tweaks.CheckWhitelist(other)) return;
        var potentialHealthManager = other.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            if (PlayerData._instance.silk >= 9)
            {
                potentialHealthManager.invincible = false;
                potentialHealthManager.hp -= (int) (PlayerData._instance.nailDamage * PlayerData._instance.silk * nailparryeverythingPlugin.PARRY_DAMAGE_MULTIPLIER);
                //potentialHealthManager.hp -= potentialHealthManager.hp * (PlayerData._instance.silk * (5 / 3) - 5) / 100;
                PlayerData._instance.silk = 0;
                GameManager._instance.FreezeMoment(FreezeMomentTypes.BossStun);
            } else potentialHealthManager.invincible = true;
            var fsm = PlayMakerFSM.FindFsmOnGameObject(potentialHealthManager.gameObject, "Control");
            if (fsm == null) fsm = potentialHealthManager.gameObject.GetComponent<PlayMakerFSM>();
            nailparryeverythingPlugin.log("KEY: " + fsm.Fsm.FsmComponent.name);
            nailparryeverythingPlugin.log("ACTIVE STATE NAME: " + fsm.ActiveStateName);
            if (!tweaks.IsValidActiveState(fsm.Fsm.FsmComponent.name, fsm.ActiveStateName)) return;
        }
        if (damageHero != null) nailparryeverythingPlugin.Instance.StartCoroutine(damageHero.NailClash(0, "NPE PARRY", transform.position));
        HeroController._instance.NailParry();
        GameManager._instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
        if (damageHero != null) StartCoroutine(DisableDamageHero(damageHero));
        else
        {
            HeroController._instance.StartInvulnerable(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
            nailparryeverythingPlugin.OnParry();
        }
    }

    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
        __instance.enabled = true;
    }
}