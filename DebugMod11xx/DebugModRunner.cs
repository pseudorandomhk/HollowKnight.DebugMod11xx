using System;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<string, Action> keybindActions;
    
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
        DebugMod.Instance.LogDebug("Starting debug mod runner");
        InitializeKeybinds();
        DebugMod.Instance.AcceptingInput = true;
        
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
        
        DebugMod.Instance.LogDebug("Finished debug mod runner start");
    }

    private void InitializeKeybinds()
    {
        Settings s = DebugMod.Instance.settings;
        keybindActions = new Dictionary<string, Action>
        {
            { s.toggleInfo, () => showInfo = !showInfo },
            { s.toggleInvincibility, ToggleInvincibility },
            { s.toggleInfiniteHealth, () => infiniteHealth = !infiniteHealth },
            { s.toggleInfiniteSoul, () => infiniteSoul = !infiniteSoul },
            { s.toggleVignette, () => vignetteDisabled = !vignetteDisabled },
            { s.toggleNoclip, ToggleNoclip },
            { s.cameraFollow, ToggleCameraFollow },
            { s.cycleHitboxes, CycleHitboxVisibility},
            { s.increaseLoadExtension, () => loadExtension++ },
            { s.decreaseLoadExtension, () => loadExtension-- },
            { s.zoomIn, () => GameCameras.instance.tk2dCam.ZoomFactor *= 1.05f },
            { s.zoomOut, () => GameCameras.instance.tk2dCam.ZoomFactor /= 1.05f },
            { s.resetZoom, () => GameCameras.instance.tk2dCam.ZoomFactor = 1f },
            { s.increaseTimescale, () => timescale *= 1.05f },
            { s.decreaseTimescale, () => timescale /= 1.05f },
            { s.resetTimescale, () => timescale = 1f },
            { s.saveSavestate, () => savestateManager.CreateSavestate() },
            { s.loadSavestate, () => savestateManager.StartSelectState(false) },
            { s.loadSavestateDuped, () => savestateManager.StartSelectState(true) }
        };

        DebugMod.Instance.LogInfo($"Initialized {keybindActions.Count} keybinds");
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
        
        if (DebugMod.Instance.AcceptingInput)
            foreach (var bind in keybindActions)
                if (Input.GetKeyDown(bind.Key))
                    bind.Value();

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

        if (!GameManager.instance.GetField<GameManager, bool>("timeSlowed") && Math.Abs(Time.timeScale - timescale) > 1e-5)
            Time.timeScale = timescale;
    }

    private void OnGUI()
    {
        if (GameManager.instance == null || GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE
            || !showInfo)
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