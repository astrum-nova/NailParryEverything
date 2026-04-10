using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace nailparryeverything;

[BepInAutoPlugin(id: "io.github.astrum-nova.nailparryeverything")]
public partial class nailparryeverythingPlugin : BaseUnityPlugin
{
    public static nailparryeverythingPlugin Instance { get; set; } = null!;
    public static bool ENEMY_INVINCIBILITY;
    public static bool DEBUG_INFO;
    public static bool PARRY_FREEZE;
    public static float PARRY_DAMAGE_MULTIPLIER;
    public static float PARRY_INVULNERABILITY;
    public static int SILK_GAIN_PER_PARRY;
    public static bool DEFAULT_PARRY_INVCINCIBILITY;
    public static bool firstSinnerScene;
    private void Awake()
    {
        Instance = this;
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        PARRY_DAMAGE_MULTIPLIER = Config.Bind(
            "Modifiers",
            "Parry Damage Multiplier",
            0.55f,
            "Multiplier for the parry counter, used to balance the damage output with this formula: [nail damage * silk amount (9 min, 18 max) * multiplier], otherwise it would be too strong."
        ).Value;
        PARRY_INVULNERABILITY = Config.Bind(
            "Modifiers",
            "Parry Invulnerability Time",
            0.43f,
            "The invulnerability time in seconds hornet receives against an attack for parrying it. Some parried attacks will give you global invulnerability time."
        ).Value;
        SILK_GAIN_PER_PARRY = Config.Bind(
            "Modifiers",
            "Silk Gain Per Parry",
            5,
            "The amount of silk you gain for a successfull parry, 3 amounts to 1 bar of silk."
        ).Value;
        ENEMY_INVINCIBILITY = Config.Bind(
            "Accessibility",
            "Enemies Immune To Normal Damage",
            true,
            "Whether enemies should be immune to damage with the exception of parry counters. Set this to false if you want to be able to damange enemies without parry counters."
        ).Value;
        var quickCharge = Config.Bind(
            "Accessibility",
            "Quick Nail Art Charge",
            true,
            "Quick charge for nail arts always active, basically the Pin Badge tool effect."
        ).Value;
        DEBUG_INFO = Config.Bind(
            "Debug",
            "Show Debug Info",
            true,
            "Whether to show debug information at the top left of the game."
        ).Value;
        PARRY_FREEZE = Config.Bind(
            "Accessibility",
            "Parry Freeze",
            true,
            "Whether to freeze the game when you parry something."
        ).Value; 
        DEFAULT_PARRY_INVCINCIBILITY = Config.Bind(
            "Accessibility",
            "Default Parry Invincibility",
            false,
            "The default parry invincibility is a lot less forgiving, you might need to spam sometimes or be more precise but its pretty doable."
        ).Value;
        Harmony.CreateAndPatchAll(typeof(nailparryeverythingPlugin));
        SceneManager.sceneLoaded += (scene, _) =>
        {
            firstSinnerScene = scene.name.Equals("Slab_10b"); //! I WROTE THE SCENE NAME FROM MEMORY LETS SEE IF IM THE GOAT ok im not the goat
            try
            {
                PlayerData.instance.hasChargeSlash = true;
                if (!quickCharge) return;
                HeroController.instance.NAIL_CHARGE_TIME = HeroController.instance.NAIL_CHARGE_TIME_QUICK / 2;
                HeroController.instance.NAIL_CHARGE_BEGIN_TIME = HeroController.instance.NAIL_CHARGE_BEGIN_TIME_QUICK / 2;
                if (DEBUG_INFO) SetOverlayText("Debug info is enabled, use this to help development for the mod,\ninformation about the object you just hit will appear here.\nYou can turn it off in the mod settings if you want.");
            } catch {/*ignored*/}
        };
    }
    
    //* NAIL ATTACK PATCHES
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.Awake))]
    [HarmonyPatch(typeof(DashStabNailAttack), nameof(DashStabNailAttack.Awake))]
    [HarmonyPatch(typeof(Downspike), nameof(Downspike.StartSlash))]
    private static void AddParryCollision(MonoBehaviour __instance) => __instance.gameObject.AddComponent<ParryCollision>();
    
    //* ENEMY DAMAGER PATCHES
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), nameof(DamageHero.NailClash))]
    private static void DamageHero_NailClash(DamageHero __instance)
    {
        if (__instance == null) return;
        HeroController.instance.StartInvulnerable(tweaks.HandleAdditionalIframes(__instance.gameObject));
        OnParry();
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DamageHero), nameof(DamageHero.OnEnable))]
    private static void DamageHero_OnEnable(DamageHero __instance)
    {
        __instance.canClashTink = true;
        __instance.forceParry = true;
        __instance.noClashFreeze = false;
        __instance.preventClashTink = false;
    }
    
    //* HEALTH MANAGER PATCHES
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.OnEnable))]
    private static void HealthManager_Start(HealthManager __instance) => SetHealthManagerInvincibility(__instance, true);
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Hit))]
    private static void HealthManager_Hit(HealthManager __instance, ref HitInstance hitInstance)
    {
        var collider2d = __instance.gameObject.GetComponentInParent<Collider2D>();
        if (tweaks.CheckList(collider2d, 2) && !tweaks.CheckList(collider2d, 3) || hitInstance.AttackType == AttackTypes.Spikes)
        {
            __instance.doNotGiveSilk = true;
            SetHealthManagerInvincibility(__instance, false);
        }
        else
        {
            SetHealthManagerInvincibility(__instance, true);
            __instance.InvincibleFromDirection = 0;
            __instance.invincibleFromDirection = 0;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Invincible))]
    private static void HealthManager_Invincible(HealthManager __instance, ref HitInstance hitInstance)
    {
        try
        {
            if (!hitInstance.Source.name.StartsWith("Charge Slash") &&
                !hitInstance.Source.transform.parent.name.StartsWith("Charge Slash") &&
                !hitInstance.Source.transform.parent.parent.name.StartsWith("Charge Slash")) return;
            if (PlayerData.instance.silk >= 9)
            {
                __instance.TakeDamage(new HitInstance
                {
                    Source = hitInstance.Source,
                    AttackType = AttackTypes.Spell,
                    DamageDealt = (int)(PlayerData.instance.nailDamage * PlayerData.instance.silk * PARRY_DAMAGE_MULTIPLIER),
                    Direction = hitInstance.Direction,
                    Multiplier = 1f,
                    MagnitudeMultiplier = 1f,
                    IgnoreInvulnerable = true,
                    HitEffectsType = hitInstance.HitEffectsType,
                    IsNailTag = hitInstance.IsNailTag
                });
                HeroController.instance.TakeSilk(PlayerData.instance.silk);
                GameManager.instance.FreezeMoment(FreezeMomentTypes.BossStun);
                
                //? Special treatment for first sinner my beloved to let the player break her bind
            } else if (firstSinnerScene && PlayMakerFSM.FindFsmOnGameObject(__instance.gameObject, "Control").ActiveStateName is "Bind Silk" or "Bind Heal")
            {
                __instance.TakeDamage(new HitInstance
                {
                    Source = hitInstance.Source,
                    AttackType = AttackTypes.Spell,
                    DamageDealt = 0,
                    Direction = hitInstance.Direction,
                    Multiplier = 1f,
                    MagnitudeMultiplier = 1f,
                    IgnoreInvulnerable = true,
                    HitEffectsType = hitInstance.HitEffectsType,
                    IsNailTag = hitInstance.IsNailTag
                });
            }
        } catch {/*ignored*/}
    }

    //* GAME MANAGER FREEZE PATCHE
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.FreezeMoment), typeof(FreezeMomentTypes), typeof(Action))]
    private static bool GameManager_FreezeMoment(GameManager __instance, FreezeMomentTypes type, Action onFinish) => type != FreezeMomentTypes.NailClashEffect || PARRY_FREEZE;

    //* OTHER LOGIC
    private static bool canAddSilk = true;
    public static void OnParry()
    {
        if (PlayerData.instance.silk >= PlayerData.instance.CurrentSilkMax) return;
        if (!canAddSilk) return;
        canAddSilk = false;
        Instance.StartCoroutine(AddSilkPart());
    }
    public static void SetHealthManagerInvincibility(HealthManager healthManager, bool invincibility)
    {
        var res = invincibility && !healthManager.DoNotGiveSilk && ENEMY_INVINCIBILITY;
        healthManager.invincible = res;
        if (healthManager.sendDamageTo == null) return;
        healthManager.sendDamageTo.invincible = res;
    }
    private static IEnumerator AddSilkPart()
    {
        for (var i = 0; i < SILK_GAIN_PER_PARRY; i++)
        {
            yield return new WaitForSeconds(0.05f);
            HeroController.instance.AddSilkParts(1, true);
        }
        canAddSilk = true;
    }
    
    //! DEBUG !\\
    private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("[NPE LOG]");
    public static void log(string msg) => logger.LogInfo(msg);
    private static bool keepMaxHP;
    private void FixedUpdate()
    {
        try
        {
            if (InputHandler.Instance && InputHandler.Instance.inputActions.Up && InputHandler.Instance.inputActions.DreamNail) HeroController.instance.RefillSilkToMax();
            if (InputHandler.Instance && InputHandler.Instance.inputActions.Left && InputHandler.Instance.inputActions.DreamNail) keepMaxHP = true;
            if (InputHandler.Instance && InputHandler.Instance.inputActions.Right && InputHandler.Instance.inputActions.DreamNail) keepMaxHP = false;
            if (keepMaxHP) HeroController.instance.MaxHealth();
        } catch {/*ignored*/}
    }
    private static Text? overlayText;
    private static GameObject? overlayCanvas;
    private static void CreateOverlay()
    {
        if (overlayCanvas != null) return;

        overlayCanvas = new GameObject("Nail Parry Everything Canvas");
        DontDestroyOnLoad(overlayCanvas);

        var canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        overlayCanvas.AddComponent<CanvasScaler>();
        var canvasGroup = overlayCanvas.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        var textObj = new GameObject("Nail Parry Everything Text");
        textObj.transform.SetParent(overlayCanvas.transform, false);

        overlayText = textObj.AddComponent<Text>();
        overlayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        overlayText.fontSize = 18;
        overlayText.color = Color.white;
        overlayText.alignment = TextAnchor.UpperLeft;
        var outline = overlayText.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, -1f);
        overlayText.horizontalOverflow = HorizontalWrapMode.Overflow;
        overlayText.verticalOverflow   = VerticalWrapMode.Overflow;
        var rect = overlayText.rectTransform;
        rect.anchorMin = new Vector2(0.005f, 0f);
        rect.anchorMax = new Vector2(0f, 0.8f);
        rect.pivot = new Vector2(0f, 0f);
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(610f, -10f);
        overlayText.raycastTarget = false;
    }
    public static void SetOverlayText(string text)
    {
        if (!DEBUG_INFO) return;
        if (overlayText == null) CreateOverlay();
        overlayText!.text = $"[Nail Parry Everything]\n" + text;
    }
    //! DEBUG !\\
}