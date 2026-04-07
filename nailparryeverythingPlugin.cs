using System;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace nailparryeverything;

[BepInAutoPlugin(id: "io.github.astrum-nova.nailparryeverything")]
public partial class nailparryeverythingPlugin : BaseUnityPlugin
{
    //! DEBUG
    private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("[NPE LOG]");
    public static void log(string msg) => logger.LogInfo(msg);
    //! DEBUG
    public static nailparryeverythingPlugin Instance { get; set; } = null!;
    public static bool ENEMY_INVINCIBILITY;
    public static float PARRY_DAMAGE_MULTIPLIER;
    public static float PARRY_INVULNERABILITY;
    public static int SILK_GAIN_PER_PARRY;

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
        Harmony.CreateAndPatchAll(typeof(nailparryeverythingPlugin));
        SceneManager.sceneLoaded += (_, _) =>
        {
            PlayerData.instance.hasChargeSlash = true;
            if (!quickCharge) return;
            HeroController.instance.NAIL_CHARGE_TIME = HeroController.instance.NAIL_CHARGE_TIME_QUICK;
            HeroController.instance.NAIL_CHARGE_BEGIN_TIME = HeroController.instance.NAIL_CHARGE_BEGIN_TIME_QUICK;
        };
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.Awake))]
    private static void NailSlash_Awake(NailSlash __instance) => __instance.gameObject.AddComponent<ParryCollision>();
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DashStabNailAttack), nameof(DashStabNailAttack.Awake))]
    private static void DashStabNailAttack_Awake(DashStabNailAttack __instance) => __instance.gameObject.AddComponent<ParryCollision>();
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Downspike), nameof(Downspike.StartSlash))]
    private static void Downspike_StartSlash(Downspike __instance) => __instance.gameObject.AddComponent<ParryCollision>();
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
    private static void HealthManager_Start(HealthManager __instance) => SetHealthManagerInvincibility(__instance, true);
    public static void SetHealthManagerInvincibility(HealthManager healthManager, bool invincibility)
    {
        var res = invincibility && !healthManager.DoNotGiveSilk && ENEMY_INVINCIBILITY;
        healthManager.invincible = res;
        if (healthManager.sendDamageTo == null) return;
        healthManager.sendDamageTo.invincible = res;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Hit))]
    private static void HealthManager_Hit(HealthManager __instance, ref HitInstance hitInstance)
    {
        SetHealthManagerInvincibility(__instance, true);
        __instance.InvincibleFromDirection = 0;
        __instance.invincibleFromDirection = 0;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Invincible))]
    private static void HealthManager_Invincible(HealthManager __instance, ref HitInstance hitInstance)
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
        }
    }
    public static void OnParry()
    {
        if (PlayerData.instance.silk < PlayerData.instance.silkMax) HeroController.instance.AddSilkParts(SILK_GAIN_PER_PARRY, true);
    }
    //! DEBUG !\\
    private static bool keepMaxHP;
    private void FixedUpdate()
    {
        if (InputHandler.Instance && InputHandler.Instance.inputActions.Up && InputHandler.Instance.inputActions.DreamNail) HeroController.instance.RefillSilkToMax();
        if (InputHandler.Instance && InputHandler.Instance.inputActions.Left && InputHandler.Instance.inputActions.DreamNail) keepMaxHP = true;
        if (InputHandler.Instance && InputHandler.Instance.inputActions.Right && InputHandler.Instance.inputActions.DreamNail) keepMaxHP = false;
        if (keepMaxHP) HeroController.instance.MaxHealth();
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
        if (overlayText == null) CreateOverlay();
        overlayText!.text = $"[Nail Parry Everything]\n" + text;
    }
    //! DEBUG !\\
}