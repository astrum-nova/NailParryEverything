using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace nailparryeverything;

public class ParryCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //nailparryeverythingPlugin.log("TRIGGER: " + other.gameObject.name);
        var damageHero = other.gameObject.GetComponentInParent<DamageHero>();
        if (damageHero == null || !damageHero.enabled) return;
        var potentialHealthManager = other.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            //potentialHealthManager.invincible = true;
            //TODO:
            //1. GET THE CONTROL FSM FROM THE HEALTH MANAGER AND FIND ITS NAME
            //2. MATCH THE NAME WITH A LIST OF BOSS NAMES YOU WANT TO ADD TWEAKS TO
            //3. GET THE LIST OF ALLOWED STATES RELATED TO THIS BOSS FSM
            //4. IF THE CURRENT FSM ACTIVE STATE MATCHES ONE OF THOSE IN THE LIST OF ALLOWED STATES ALLOW A PARRY
        }
        else
        {
            nailparryeverythingPlugin.Instance.StartCoroutine(damageHero.NailClash(0, "NPE PARRY", transform.position));
            HeroController._instance.NailParry();
            GameManager._instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
            StartCoroutine(DisableDamageHero(damageHero));
            HeroController._instance.StartInvulnerable(HandleAdditionalIframes(other));
        }
    }

    private static float HandleAdditionalIframes(Collider2D other)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        
        //! DEBUG
        nailparryeverythingPlugin.log("\n/////////////////////////////////////////////////////\n" + "gameObject.name: " + gameObject.name + "\n" + "parent.name: " + (parent != null ? parent.name : "NULL-PARENT") + "\n" + "root.name: " + (root != null ? root.name : "NULL-ROOT") + "\n" + @"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
        //! DEBUG
        
        //* Skull Tyrant
        if (root.name.StartsWith("Bone_Boulder SK")) return 0.35f;
        
        return 0;
    }
    
    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(nailparryeverythingPlugin.PARRY_INVULNERABILITY);
        __instance.enabled = true;
    }
}