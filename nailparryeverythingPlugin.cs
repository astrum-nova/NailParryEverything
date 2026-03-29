using System;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
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
            0.5f,
            "The invulnerability time in seconds hornet receives after a successful parry. (default is 0.5)"
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
        //TODO: 
        //1: NARROW DOWN THE SLASH THINGS FOR HORNET ONLY
        //2: THIS HAPPENS FOR A FEW FRAMES EVERY TIME, MAKE IT SO IT ONLY HAPPENS ONCE AND IGNORES THE NEXT FRAMES
        //3: ONLY IF THE DAMAGE HERO INSTANCE IS ON THE ATTACK LAYER

        if (!collision.gameObject.name.Contains("Slash")) return;
        if () return;
        Instance.StartCoroutine(__instance.NailClash(0, "NPE PARRY", collision.transform.position));
        HeroController._instance.NailParry();
        GameManager._instance.FreezeMoment(FreezeMomentTypes.NailClashEffect);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DamageHero), "OnEnable")]
    private static void DamageHero_OnEnable(DamageHero __instance)
    {
        /*__instance.canClashTink = true;
        __instance.forceParry = true;
        __instance.noClashFreeze = false;
        __instance.preventClashTink = false;*/
    }
}