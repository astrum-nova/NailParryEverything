using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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
    public static float PARRY_DAMAGE_MULTIPLIER;
    public static float PARRY_INVULNERABILITY;
    public static int SILK_GAIN_PER_PARRY;

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
            0.45f,
            "This value is used to determine the fraction of your nail damage to be dealt to an enemy for a successful parry. (default is 0.45)"
        ).Value;
        PARRY_INVULNERABILITY = Config.Bind(
            "Nail Parry Everything",
            "Parry Invulnerability",
            0.3f,
            "The invulnerability time in seconds hornet receives against an attack for parrying it. (default is 0.3)"
        ).Value;
        SILK_GAIN_PER_PARRY = Config.Bind(
            "Nail Parry Everything",
            "Silk Gain Per Parry",
            1,
            "The amount of silk you gain for a successfull parry. (default is 2)"
        ).Value;
        Harmony.CreateAndPatchAll(typeof(nailparryeverythingPlugin));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.Awake))]
    private static void NailSlash_Awake(NailSlash __instance) => __instance.gameObject.AddComponent<ParryCollision>();
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DashStabNailAttack), nameof(DashStabNailAttack.Awake))]
    private static void DashStabNailAttack_Awake(DashStabNailAttack __instance) => __instance.gameObject.AddComponent<ParryCollision>();
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), nameof(DamageHero.NailClash))]
    private static void DamageHero_NailClash(DamageHero __instance)
    {
        HeroController.instance.StartInvulnerable(tweaks.HandleAdditionalIframes(__instance.gameObject));
        OnParry();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), "OnEnable")]
    private static void DamageHero_OnEnable(DamageHero __instance)
    {
        //if (__instance.gameObject.GetComponentInParent<HealthManager>() != null) return;
        __instance.canClashTink = true;
        __instance.forceParry = true;
        __instance.noClashFreeze = false;
        __instance.preventClashTink = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), "Start")]
    private static void HealthManager_OneEnable(HealthManager __instance) => SetHealthManagerInvincibility(__instance, true);

    public static void SetHealthManagerInvincibility(HealthManager healthManager, bool invincibility)
    {
        healthManager.invincible = invincibility;
        healthManager.immuneToNailAttacks = invincibility;
        if (healthManager.sendDamageTo == null) return;
        healthManager.sendDamageTo.invincible = invincibility;
        healthManager.sendDamageTo.immuneToNailAttacks = invincibility;
    }

    public static void OnParry()
    {
        if (PlayerData.instance.silk < PlayerData.instance.CurrentSilkMax) HeroController.instance.AddSilk(SILK_GAIN_PER_PARRY, true);
    } 
}