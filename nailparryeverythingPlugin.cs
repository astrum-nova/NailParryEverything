using System;
using System.Collections;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using tk2dRuntime.TileMap;
using UnityEngine;

namespace nailparryeverything;

[BepInAutoPlugin(id: "io.github.astrum-nova.nailparryeverything")]
public partial class nailparryeverythingPlugin : BaseUnityPlugin
{
    //! DEBUG
    private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("[NPE LOG]");
    private static void log(string msg) => logger.LogInfo(msg);
    //! DEBUG
    private static nailparryeverythingPlugin Instance { get; set; } = null!;
    
    private static bool PARRY_DAMAGES_ENEMY;
    private static float PARRY_DAMAGE_MULTIPLIER;
    private static float PARRY_INVULNERABILITY;

    private static readonly string[] hornetSlashes = [
        "Slash",
        "DownSlash",
        "UpSlash",
        "WallSlash"
    ];

    private void Awake()
    {
        Instance = this;
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        PARRY_DAMAGES_ENEMY = Config.Bind(
            "Nail Parry Everything",
            "Parry Damages Enemies",
            true,
            "If true, successful parries will deal a fraction of your nail damage to the enemy. (default is true)"
        ).Value;
        PARRY_DAMAGE_MULTIPLIER = Config.Bind(
            "Nail Parry Everything",
            "Parry Damage Multiplier",
            0.33f,
            "This value is used to determine the fraction of your nail damage to be dealt to an enemy for a successful parry. (default is 0.33)"
        ).Value;
        PARRY_INVULNERABILITY = Config.Bind(
            "Nail Parry Everything",
            "Parry Invulnerability",
            0.4f,
            "The invulnerability time in seconds hornet receives against an attack for parrying it. (default is 0.4)"
        ).Value;
        Harmony.CreateAndPatchAll(typeof(nailparryeverythingPlugin));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DamageHero), "OnCollisionEnter2D")]
    private static void DamageHero_OnCollisionEnter2D(DamageHero __instance, Collider2D collision)
    {
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DamageHero), "OnTriggerEnter2D")]
    private static void DamageHero_OnTriggerEnter2D(DamageHero __instance, Collider2D collision)
    {
        if (!hornetSlashes.Contains(collision.gameObject.name) || !__instance.enabled) return;
        var potentialHealthManager = __instance.gameObject.GetComponentInParent<HealthManager>();
        if (potentialHealthManager != null && !potentialHealthManager.doNotGiveSilk)
        {
            //potentialHealthManager.invincible = true;
            //TODO:
            //1. GET THE CONTROL FSM FROM THE HEALTH MANAGER AND FIND ITS NAME
            //2. MATCH THE NAME WITH A LIST OF BOSS NAMES YOU WANT TO ADD TWEAKS TO
            //3. GET THE LIST OF ALLOWED STATES RELATED TO THIS BOSS FSM
            //4. IF THE CURRENT FSM ACTIVE STATE MATCHES ONE OF THOSE IN THE LIST OF ALLOWED STATES ALLOW A PARRY
        }
        //else
        {
            Instance.StartCoroutine(__instance.NailClash(0, "NPE PARRY", collision.transform.position));
            HeroController._instance.NailParry();
            GameManager._instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
            Instance.StartCoroutine(DisableDamageHero(__instance));
            //TODO:
            //1. MAKE A HASMAP STRING->FLOAT OF ALL THE ATTACKS THAT NEED HORNET IFRAMES AND MATCH IT WITH THE CORRESPONDING IFRAMES NEEDED
            // FOR EXAMPLE map{("down dash", 0.3f), ("skull tyrant falling boulder", 0.2f)}
            // USE THE SAME NAMES OF THE DAMAGE HERO INSTANCE FOR THE KEYS
            /* * HeroController._instance.StartInvulnerable(PARRY_INVULNERABILITY); * */
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), "OnEnable")]
    private static void DamageHero_OnEnable(DamageHero __instance)
    {
        __instance.canClashTink = true;
        __instance.forceParry = true;
        __instance.noClashFreeze = false;
        __instance.preventClashTink = false;
    }

    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(PARRY_INVULNERABILITY);
        __instance.enabled = true;
    }
}