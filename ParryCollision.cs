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
            var fsm = PlayMakerFSM.FindFsmOnGameObject(potentialHealthManager.gameObject, "Control");
            if (fsm == null) fsm = potentialHealthManager.gameObject.GetComponent<PlayMakerFSM>();
            if (PlayerData._instance.silk >= 9 && fsm.Fsm.name == "Control") //TODO: EXCLUDE WHILE PARRYING TOO CHECK WITH MOORWING OR WIDOW
            {
                potentialHealthManager.invincible = false;
                //nailparryeverythingPlugin.SetHealthManagerInvincibility(potentialHealthManager, false);
                potentialHealthManager.hp -= (int) (PlayerData._instance.nailDamage * PlayerData._instance.silk * nailparryeverythingPlugin.PARRY_DAMAGE_MULTIPLIER);
                if (potentialHealthManager.hp < 1) potentialHealthManager.hp = 1;
                PlayerData._instance.silk = 0;
                GameManager._instance.FreezeMoment(FreezeMomentTypes.BossStun);
            }
            potentialHealthManager.invincible = true;
            nailparryeverythingPlugin.log("[NAIL PARRY EVERYTHING NEEDLE HIT]");
            nailparryeverythingPlugin.log("INTERNAL BOSS NAME: \"" + fsm.name + "\"");
            nailparryeverythingPlugin.log("ACTIVE STATE NAME:  \"" + fsm.ActiveStateName + "\"");
            nailparryeverythingPlugin.log("AI TYPE:            \"" + fsm.Fsm.name + "\" (this should be \"Control\")");
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
        //TODO: IF THE PARENT OBJECT GETS DESTROYED AND WE REENABLE THIS IT BREAKS, CHECK WITH PUTRIFIED DUCT SQUITS
        __instance.enabled = true;
    }
}