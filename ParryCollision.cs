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
        var names = tweaks.getGameObjectParentRootNames(other.gameObject);
        nailparryeverythingPlugin.log("TRIGGER:");
        nailparryeverythingPlugin.log("name: " + names[0]);
        nailparryeverythingPlugin.log("parent: " + names[1]);
        nailparryeverythingPlugin.log("root: " + names[2]);
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if ((damageHero == null || !damageHero.enabled || tweaks.CheckBlacklist(other)) && !tweaks.CheckWhitelist(other)) return;
        var potentialHealthManager = other.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            //if (potentialHealthManager.hp > 400) potentialHealthManager.invincible = true;
            //TODO:
            //1. GET THE CONTROL FSM FROM THE HEALTH MANAGER AND FIND ITS NAME
            //2. MATCH THE NAME WITH A LIST OF BOSS NAMES YOU WANT TO ADD TWEAKS TO
            //3. GET THE LIST OF ALLOWED STATES RELATED TO THIS BOSS FSM
            //4. IF THE CURRENT FSM ACTIVE STATE MATCHES ONE OF THOSE IN THE LIST OF ALLOWED STATES ALLOW A PARRY
        }
        //else
        {
            if (damageHero != null) nailparryeverythingPlugin.Instance.StartCoroutine(damageHero.NailClash(0, "NPE PARRY", transform.position));
            HeroController._instance.NailParry();
            GameManager._instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
            if (damageHero != null) StartCoroutine(DisableDamageHero(damageHero));
            else
            {
                HeroController._instance.StartInvulnerable(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
            }
        }
    }

    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
        __instance.enabled = true;
    }
}