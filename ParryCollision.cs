using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace nailparryeverything;

public class ParryCollision : MonoBehaviour
{
    public bool canFreeze = true;
    private Coroutine? disableDamageHeroRoutine;
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

            //! DEBUG !\\
            var dbg = tweaks.getGameObjectParentRootNames(other.gameObject);
            if (nailparryeverythingPlugin.DEBUG_INFO) try { nailparryeverythingPlugin.SetOverlayText("INTERNAL ENEMY NAME: \"" + fsm.name + "\"\n" + "ACTIVE STATE NAME: \"" + fsm.ActiveStateName + "\"\n" + "AI TYPE: \"" + fsm.Fsm.name + "\"\n" + "GAME OBJECT: \"" + dbg[0] + "\"\n" + "PARENT OBJECT: \"" + dbg[1] + "\"\n" + "ROOT OBJECT: \"" + dbg[2] + "\""); } catch { nailparryeverythingPlugin.SetOverlayText("ERROR TRYING TO SET DEBUG TEXT"); }
            //! DEBUG !\\
            
            //? If the enemy related to this HealthManager isnt in a parryable active state, stop the function
            if (fsm != null && !tweaks.IsValidActiveState(fsm.name.Contains('(') ? fsm.name[..(fsm.name.IndexOf('(') - (fsm.name.EndsWith("(Clone)") ? 0 : 1))] : fsm.name, fsm.ActiveStateName) && !tweaks.CheckList(other, 1)) return;
        }
        
        //! DEBUG !\\
        else if (nailparryeverythingPlugin.DEBUG_INFO) try
        {
            var dbg = tweaks.getGameObjectParentRootNames(other.gameObject);
            nailparryeverythingPlugin.SetOverlayText("GAME OBJECT: \"" + dbg[0] + "\"\n" +"PARENT OBJECT: \"" + dbg[1] + "\"\n" + "ROOT OBJECT: \"" + dbg[2] + "\"");
        } catch { nailparryeverythingPlugin.SetOverlayText("ERROR TRYING TO SET DEBUG TEXT"); }
        //! DEBUG !\\
        
        //! DAMAGE HERO
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if ((damageHero == null || tweaks.CheckList(other, 0)) && !tweaks.CheckList(other, 1)) return;
        if (damageHero != null)
        {
            if (damageHero.nailClashRoutine != null) StopCoroutine(damageHero.nailClashRoutine);
            StartCoroutine(damageHero.NailClash(GetComponentInParent<DamageEnemies>().direction, "Nail Attack", transform.position));
            if (!nailparryeverythingPlugin.DEFAULT_PARRY_INVCINCIBILITY)
            {
                if (disableDamageHeroRoutine != null) StopCoroutine(disableDamageHeroRoutine);
                disableDamageHeroRoutine = StartCoroutine(DisableDamageHero(damageHero));
            }
        }
        else
        {
            HeroController.instance.parryInvulnTimer = nailparryeverythingPlugin.PARRY_INVULNERABILITY;
            nailparryeverythingPlugin.OnParry();
        }
        if (canFreeze && nailparryeverythingPlugin.PARRY_FREEZE)
        {
            canFreeze = false;
            nailparryeverythingPlugin.Instance.StartCoroutine(ImJustGettingADrink());
            GameManager.instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
        }
    }
    private static IEnumerator DisableDamageHero(DamageHero damageHero)
    {
        damageHero.enabled = false;
        yield return new WaitForSeconds(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
        damageHero.enabled = true;
    }
    private IEnumerator ImJustGettingADrink()
    {
        yield return new WaitForSeconds(0.3f);
        canFreeze = true;
    }
}