using System;
using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace nailparryeverything;

public class ParryCollision : MonoBehaviour
{
    public static bool canFreeze = true;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name ==  "HeroBox" || other.gameObject.name.StartsWith("Enviro Region Simple") || other.gameObject.name.StartsWith("Scene")) return;
        //! HEALTH MANAGER
        var potentialHealthManager = other.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            nailparryeverythingPlugin.SetHealthManagerInvincibility(potentialHealthManager, true);
            var fsm = PlayMakerFSM.FindFsmOnGameObject(potentialHealthManager.gameObject, "Control");
            if (fsm == null) fsm = potentialHealthManager.gameObject.GetComponent<PlayMakerFSM>();
            var dbg = tweaks.getGameObjectParentRootNames(other.gameObject);

            try
            {
                logfsmdata(fsm);
                nailparryeverythingPlugin.SetOverlayText("INTERNAL ENEMY NAME: \"" + fsm.name + "\"\n" +
                                                         "ACTIVE STATE NAME: \"" + fsm.ActiveStateName + "\"\n" +
                                                         "AI TYPE: \"" + fsm.Fsm.name + "\"\n" +
                                                         "GAME OBJECT: \"" + dbg[0] + "\"\n" +
                                                         "PARENT OBJECT: \"" + dbg[1] + "\"\n" +
                                                         "ROOT OBJECT: \"" + dbg[2] + "\"");
            }
            catch
            {
                nailparryeverythingPlugin.SetOverlayText("ERROR TRYING TO SET DEBUG TEXT");
            }
            
            //? If the enemy related to this HealthManager isnt in a parryable active state, stop the function
            if (fsm != null && !tweaks.IsValidActiveState(fsm.name.Contains('(') ? fsm.name[..(fsm.name.IndexOf('(') - (fsm.name.EndsWith("(Clone)") ? 0 : 1))] : fsm.name, fsm.ActiveStateName) && !tweaks.CheckList(other, true)) return;
        }
        else
        {
            try
            {
                var dbg = tweaks.getGameObjectParentRootNames(other.gameObject);
                nailparryeverythingPlugin.SetOverlayText("GAME OBJECT: \"" + dbg[0] + "\"\n" +
                                                         "PARENT OBJECT: \"" + dbg[1] + "\"\n" +
                                                         "ROOT OBJECT: \"" + dbg[2] + "\"");
            }
            catch
            {
                nailparryeverythingPlugin.SetOverlayText("ERROR TRYING TO SET DEBUG TEXT");
            }
        }
        //! DAMAGE HERO
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if ((damageHero == null || !damageHero.enabled || tweaks.CheckList(other, false)) && !tweaks.CheckList(other, true)) return;
        if (damageHero != null) nailparryeverythingPlugin.Instance.StartCoroutine(damageHero.NailClash(0, "NPE PARRY", transform.position));
        HeroController.instance.NailParry();
        if (canFreeze)
        {
            canFreeze = false;
            nailparryeverythingPlugin.Instance.StartCoroutine(ImJustGettingADrink());
            GameManager.instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
        }
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
    private static IEnumerator ImJustGettingADrink()
    {
        yield return new WaitForSeconds(0.3f);
        canFreeze = true;
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