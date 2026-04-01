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
    public static void log(string msg) => logger.LogInfo(msg);
    //! DEBUG
    public static nailparryeverythingPlugin Instance { get; set; } = null!;
    
    private static bool PARRY_DAMAGES_ENEMY;
    private static float PARRY_DAMAGE_MULTIPLIER;
    public static float PARRY_INVULNERABILITY;

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.Awake))]
    private static void NailSlash_StartSlash(NailSlash __instance)
    {
        __instance.gameObject.AddComponent<ParryCollision>();
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), nameof(DamageHero.NailClash))]
    private static void DamageHero_NailClash(DamageHero __instance)
    {
        log(@"/// ENTERING DamageHero.NailClash FROM nailparryeverythingPlugin.DamageHero_NailClash");
        //log("__instance.gameObject.name:  " + __instance.gameObject.name);
        //log("__instance.gameObject.transform.parent.name:  " + __instance.gameObject.transform.parent.name);
        //log("__instance.gameObject.transform.root.name:  " + __instance.gameObject.transform.root.name);
        HeroController._instance.StartInvulnerable(HandleAdditionalIframes(__instance.GetComponentInParent<Collider2D>()));
    }
    
    private static float HandleAdditionalIframes(Collider2D other)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        
        //! DEBUG
        //log("\n/////////////////////////////////////////////////////\n" + "gameObject.name: " + gameObject.name + "\n" + "parent.name: " + (parent != null ? parent.name : "NULL-PARENT") + "\n" + "root.name: " + (root != null ? root.name : "NULL-ROOT") + "\n" + @"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
        //! DEBUG
        
        //* Skull Tyrant
        if (root.name.StartsWith("Bone_Boulder SK")) return 0.35f;
        //* Widow
        if (gameObject.name.StartsWith("Spinner AtkBell")) return 0.35f;
        
        return 0;
    }
    
    private static IEnumerator DisableDamageHero(DamageHero __instance)
    {
        __instance.enabled = false;
        yield return new WaitForSeconds(PARRY_INVULNERABILITY);
        __instance.enabled = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), "OnEnable")]
    private static void DamageHero_OnEnable(DamageHero __instance)
    {
        __instance.canClashTink = true;
        __instance.forceParry = true;
        __instance.noClashFreeze = false;
        __instance.preventClashTink = false;
        
        //TODO: TRY TO USE THE ON CLASH EVENTS WRAPPER AT THE BOTTOM OF THE DamageHero CLASS
    }
}