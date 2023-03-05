using System;
using System.Collections.Generic;
using DebugMod.Hitboxes;
using DebugMod.Savestates;
using GlobalEnums;
using Modding;
using Modding.Utils;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod;

public class DebugModRunner : MonoBehaviour
{
    public static DebugModRunner Instance { get; private set; } = null;
    
    public bool AcceptingInput { get; internal set; }
    
    private Dictionary<string, List<Action>> keybindActions;
    
    private bool infiniteHealth;
    private bool infiniteSoul;
    private bool shoudBeInvincible;
    private bool vignetteDisabled;
    private bool noclip;
    private bool cameraFollow;
    private bool showInfo;

    private int loadExtension = 1;

    private float timescale = 1f;

    private Vector3 noclipPos;

    private SavestateManager savestateManager;
    private HitboxCameraController hitboxCameraController;

    private void Start()
    {
        if (Instance != null)
        {
            DebugMod.Instance.LogWarn("Debug mod runner instantiated multiple times!");
            return;
        }
        
        DebugMod.Instance.LogDebug("Starting debug mod runner");
        Instance = this;
        AcceptingInput = true;

        try
        {
            InitializeKeybinds();
        }
        catch (Exception e)
        {
            DebugMod.Instance.LogWarn($"Exception while initializing keybinds:\n{e}");
        }

        try
        {
            savestateManager = this.gameObject.AddComponent<SavestateManager>();
            hitboxCameraController = this.gameObject.AddComponent<HitboxCameraController>();
        }
        catch (Exception e)
        {
            DebugMod.Instance.LogError($"Exception while starting component manager:\n{e}");
        }

        ModHooks.TakeHealth += damage => infiniteHealth ? 0 : damage;
        ModHooks.BeforeAdditiveLoad += _ => new[] { new WaitForSeconds(Math.Max(0, loadExtension)) };

        ModHooks.ColliderCreate += HitboxHelper.AttachHitboxRenderer;
        USceneManager.sceneLoaded += (_, _) => HitboxHelper.AddSceneHitboxRenderers();

        ModHooks.HeroStart += () =>
            GameCameras.instance.cameraController.cam.cullingMask &=
                ~(1 << HitboxHelper.REG_LAYER | 1 << HitboxHelper.MISC_LAYER);

        On.GameManager.ReturnToMainMenu += (orig, self) =>
        {
            var herobox = Util.GetHeroBox();
            if (PlayerData.instance.isInvincible && !herobox.activeSelf)
            {
                PlayerData.instance.isInvincible = false;
                shoudBeInvincible = false;
                herobox.SetActive(true);
            }

            return orig(self);
        };

        DebugMod.Instance.LogDebug("Finished debug mod runner start");
    }

    private void InitializeKeybinds()
    {
        DebugMod.Instance.LogDebug("Initializing keybinds");
        Settings s = DebugMod.Instance.settings;
        keybindActions = new Dictionary<string, List<Action>>();
        var keybindFunctionalities = getFunctionalities();
        int nBinds = 0;
        
        foreach (var keybind in typeof(Settings).GetFields())
        {
            if (!keybindFunctionalities.ContainsKey(keybind.Name))
            {
                DebugMod.Instance.LogDebug($"Skipping bind for settings entry {keybind.Name}");
                continue;
            }

            string boundKey = (string)keybind.GetValue(s);
            if (keybindActions.ContainsKey(boundKey))
                keybindActions[boundKey].Add(keybindFunctionalities[keybind.Name]);
            else
                keybindActions.Add(boundKey, new List<Action>(new[] { keybindFunctionalities[keybind.Name] }));
            DebugMod.Instance.LogDebug($"{keybind.Name} bound to {boundKey}");
            nBinds++;
        }

        DebugMod.Instance.LogInfo($"Initialized {nBinds} keybinds");
    }
    
    private Dictionary<string, Action> getFunctionalities()
    {
        return new Dictionary<string, Action>
        {
            { "toggleInfo", () => showInfo = !showInfo },
            { "toggleInvincibility", ToggleInvincibility },
            { "toggleInfiniteHealth", () => infiniteHealth = !infiniteHealth },
            { "toggleInfiniteSoul", () => infiniteSoul = !infiniteSoul },
            { "toggleVignette", () => vignetteDisabled = !vignetteDisabled },
            { "toggleNoclip", ToggleNoclip },
            { "cameraFollow", ToggleCameraFollow },
            { "cycleHitboxes", CycleHitboxVisibility },
            { "increaseLoadExtension", () => loadExtension++ },
            { "decreaseLoadExtension", () => loadExtension-- },
            { "zoomIn", () => GameCameras.instance.tk2dCam.ZoomFactor *= 1.05f },
            { "zoomOut", () => GameCameras.instance.tk2dCam.ZoomFactor /= 1.05f },
            { "resetZoom", () => GameCameras.instance.tk2dCam.ZoomFactor = 1f },
            { "increaseTimescale", () => timescale *= 1.05f },
            { "decreaseTimescale", () => timescale /= 1.05f },
            { "resetTimescale", () => timescale = 1f },
            { "saveSavestate", () => savestateManager.CreateSavestate() },
            { "loadSavestate", () => savestateManager.StartSelectState(false) },
            { "loadSavestateDuped", () => savestateManager.StartSelectState(true) },
            {
                "toggleCollision",
                () => HeroController.instance.GetComponent<Rigidbody2D>().isKinematic =
                    !HeroController.instance.GetComponent<Rigidbody2D>().isKinematic
            },
            { "toggleDreamgateInvuln", ToggleDgateInvuln }
        };
    }

    private void ToggleDgateInvuln()
    {
        var herobox = Util.GetHeroBox();
        bool isInvuln = PlayerData.instance.isInvincible && !herobox.activeSelf;

        PlayerData.instance.isInvincible = !isInvuln;
        herobox.SetActive(isInvuln);
    }

    private void ToggleNoclip()
    {
        noclip = !noclip;
        if (noclip)
            noclipPos = HeroController.instance.gameObject.transform.position;
    }

    private void ToggleCameraFollow()
    {
        cameraFollow = !cameraFollow;
        GameManager.instance.cameraCtrl.SetField("isGameplayScene", GameManager.instance.IsGameplayScene());
    }

    private void ToggleInvincibility()
    {
        PlayerData.instance.isInvincible = !PlayerData.instance.isInvincible;
        shoudBeInvincible = PlayerData.instance.isInvincible;
    }

    private void CycleHitboxVisibility()
    {
        hitboxCameraController.CycleHitboxVisibility();
    }

    private void Update()
    {
        if (GameManager.instance == null || GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            return;
        
        if (AcceptingInput)
            foreach (var bind in keybindActions)
                if (Input.GetKeyDown(bind.Key))
                    foreach (var func in bind.Value)
                        func();

        if (infiniteSoul)
            HeroController.instance.AddMPCharge(999);

        if (shoudBeInvincible)
            PlayerData.instance.isInvincible = true;
        
        HeroController.instance.vignette.enabled = !vignetteDisabled;

        if (noclip)
        {
            if (GameManager.instance.inputHandler.inputActions.left.IsPressed)
                noclipPos += Vector3.left * (Time.deltaTime * 20f);
            if (GameManager.instance.inputHandler.inputActions.right.IsPressed)
                noclipPos += Vector3.right * (Time.deltaTime * 20f);
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                noclipPos += Vector3.up * (Time.deltaTime * 20f);
            if (GameManager.instance.inputHandler.inputActions.down.IsPressed)
                noclipPos += Vector3.down * (Time.deltaTime * 20f);

            if (HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION
                && GameManager.instance.GetSceneNameString() != SavestateManager.DUMMY_SCENE_NAME)
                HeroController.instance.gameObject.transform.position = noclipPos;
            else
                noclipPos = HeroController.instance.gameObject.transform.position;
        }

        if (cameraFollow)
        {
            GameManager.instance.cameraCtrl.SetField("isGameplayScene", false);
            var hcPos = HeroController.instance.gameObject.transform.position;
            var camTargetTransform = GameManager.instance.cameraCtrl.camTarget.transform;
            var camCtrlTransform = GameManager.instance.cameraCtrl.transform;
            camTargetTransform.position = new Vector3(hcPos.x, hcPos.y, camTargetTransform.position.z);
            camCtrlTransform.position = new Vector3(hcPos.x, hcPos.y, camCtrlTransform.position.z);
        }

        if (!GameManager.instance.GetField<GameManager, bool>("timeSlowed") 
            && Time.timeScale != 0f && Math.Abs(Time.timeScale - timescale) > 1e-5)
            Time.timeScale = timescale;
    }

    private void OnGUI()
    {
        if (!showInfo || GameManager.instance == null || GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            return;

        string[] info = {
            "Active scene: " + GameManager.instance.GetSceneNameString(),
            "Bench scene: " + PlayerData.instance.respawnScene,
            "Hero position: " + (Vector2) HeroController.instance.gameObject.transform.position,
            "Hero velocity: " + HeroController.instance.gameObject.GetComponent<Rigidbody2D>().velocity,
            "Soul: " + PlayerData.instance.MPCharge,
            "Timescale: " + Time.timeScale,
            "Load extension: " + loadExtension
        };

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), String.Join("\n", info), DebugMod.Style);
    }
}