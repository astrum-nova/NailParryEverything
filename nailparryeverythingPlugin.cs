using System;
using BepInEx;
using BepInEx.Logging;
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
    public static KeyCode COUNTER_KEY = KeyCode.LeftShift;
    public static bool AUTO_COUNTER;
    public static bool ENEMY_INVINCIBILITY;
    public static float PARRY_DAMAGE_MULTIPLIER;
    public static float PARRY_INVULNERABILITY;
    public static int SILK_GAIN_PER_PARRY;

    private void Awake()
    {
        Instance = this;
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        if (!KeyCode.TryParse(Config.Bind(
                "Keybinds",
                "Parry Counter Key",
                "LeftShift",
                "Key used for a parry counter, attack while holding this and having enough silk to heal to deal damage."
            ).Value, out COUNTER_KEY)) COUNTER_KEY = KeyCode.LeftShift;
        AUTO_COUNTER = Config.Bind(
            "Accessibility",
            "Auto Parry Counter",
            false,
            "Perform a parry counter as soon as possible automatically, without needing to hold the counter key (turn this on if youre playing on controller)."
        ).Value;
        PARRY_DAMAGE_MULTIPLIER = Config.Bind(
            "Modifiers",
            "Parry Damage Multiplier",
            0.45f,
            "This value is used to determine the fraction of your nail damage times the amount of silk you have to be dealt to an enemy for a successful parry. (nail damage * silk amount (min 9, max 18) * multiplier + nail damage)"
        ).Value;
        PARRY_INVULNERABILITY = Config.Bind(
            "Modifiers",
            "Parry Invulnerability",
            0.43f,
            "The invulnerability time in seconds hornet receives against an attack for parrying it."
        ).Value;
        SILK_GAIN_PER_PARRY = Config.Bind(
            "Modifiers",
            "Silk Gain Per Parry",
            1,
            "The amount of silk you gain for a successfull parry."
        ).Value;
        ENEMY_INVINCIBILITY = Config.Bind(
            "Accessibility",
            "Enemies Immune To Normal Damage",
            false,
            "Wether enemies should be immune to damage with the exception of parry counters."
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
    private static void HealthManager_OneEnable(HealthManager __instance) => SetHealthManagerInvincibility(__instance, true);

    public static void SetHealthManagerInvincibility(HealthManager healthManager, bool invincibility)
    {
        var res = invincibility && !healthManager.DoNotGiveSilk;
        healthManager.invincible = res;
        healthManager.immuneToNailAttacks = res;
        if (healthManager.sendDamageTo == null) return;
        healthManager.sendDamageTo.invincible = res;
        healthManager.sendDamageTo.immuneToNailAttacks = res;
    }

    public static void OnParry()
    {
        if (PlayerData.instance.silk < PlayerData.instance.CurrentSilkMax) HeroController.instance.AddSilk(SILK_GAIN_PER_PARRY, true);
    }

    //! DEBUG !\\
    private void FixedUpdate()
    {
        if (InputHandler.Instance.inputActions.SuperDash && InputHandler.Instance.inputActions.DreamNail) HeroController.instance.RefillSilkToMax();
    }
    private static Text? overlayText;
    private static GameObject? overlayCanvas;
    private static void CreateOverlay()
    {
        if (overlayCanvas != null) return;

        overlayCanvas = new GameObject("Nail Parry Everything Canvas");
        GameObject.DontDestroyOnLoad(overlayCanvas);

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
        overlayText.fontSize = 24;
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