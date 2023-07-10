using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modding.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.Savestates;

public class SavestateManager : MonoBehaviour
{
    public const string DUMMY_SCENE_NAME = "Room_Mender_House";
    private const int NUM_SHOWN_STATES = 10;

    private static readonly string savestatesFolder = Path.Combine(Application.persistentDataPath, "Savestates11xx");

    private MenuState menuState;
    private List<string> allSavestates, curSelection;
    private string query;
    private int selectedSavestateIndex;
    private string lastSavestate;
    private bool isLoadStateMenuFirstFrame;

    private void Start()
    {
        DebugMod.Instance.LogDebug("Starting savestate manager");
        allSavestates = new List<string>();
        curSelection = new List<string>();
        menuState = MenuState.None;

        if (!Directory.Exists(savestatesFolder))
        {
            Directory.CreateDirectory(savestatesFolder);
            DebugMod.Instance.LogInfo($"Savestates folder not found, creating at {savestatesFolder}");
        }

        if (DebugMod.Instance.settings.deleteTempSavestatesOnStartup)
        {
            string[] tempStates = Directory.GetFiles(savestatesFolder, "__TEMP_STATE_*.json");
            if (tempStates.Length > 0)
            {
                DebugMod.Instance.LogInfo($"Deleting {tempStates.Length} temporary savestate file(s)");
                foreach (string savestate in tempStates)
                {
                    DebugMod.Instance.LogDebug($"Deleting savestate {savestate}");
                    File.Delete(savestate);
                }
            }
        }
        
        DebugMod.Instance.LogDebug("Finished starting savestate manager");
    }

    private void Update()
    {
        if (menuState == MenuState.None || GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            return;

        if (!GameManager.instance.IsGamePaused() && menuState != MenuState.None)
        {
            menuState = MenuState.None;
            isLoadStateMenuFirstFrame = false;
            DebugMod.Instance.LogWarn("Savestate menu open while game is not paused; closing");
            return;
        }
        if (isLoadStateMenuFirstFrame)
        {
            isLoadStateMenuFirstFrame = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (curSelection.Count == 0)
                return;
            lastSavestate = curSelection[selectedSavestateIndex];
            CancelMenuInput();
            StartCoroutine(LoadSavestate(menuState == MenuState.LoadStateDuped, lastSavestate));
        } 
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMenuInput();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (query.Length == 0)
                return;
            query = query.Substring(0, query.Length - 1);
            UpdateCurSelection();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && curSelection.Count > 0)
        {
            selectedSavestateIndex = selectedSavestateIndex == 0 ? curSelection.Count - 1 : selectedSavestateIndex - 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && curSelection.Count > 0)
        {
            if (curSelection.Count == 0)
                return;
            selectedSavestateIndex = selectedSavestateIndex == curSelection.Count - 1 ? 0 : selectedSavestateIndex + 1;
        }
        else if (Input.inputString.Length > 0)
        {
            query += Input.inputString;
            UpdateCurSelection();
        }
    }

    private void OnGUI()
    {
        if (menuState == MenuState.None || GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            return;

        List<string> dispLines = new()
        {
            "Select a savestate to load:",
            "> " + query
        };
        dispLines.AddRange(curSelection.Count <= NUM_SHOWN_STATES
            ? curSelection
            : curSelection.GetRange(
                Math.Min(selectedSavestateIndex, Math.Max(0, curSelection.Count - NUM_SHOWN_STATES)),
                NUM_SHOWN_STATES));
        GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, Screen.height * 0.6f),
            String.Join("\n", dispLines.Select((s, i) => i - 2 == selectedSavestateIndex ? "* " + s : s).ToArray()),
            DebugMod.Style);
    }

    public void LoadSavestateNames()
    {
        allSavestates.Clear();
        curSelection.Clear();
        query = "";
        selectedSavestateIndex = 0;

        try
        {
            allSavestates.AddRange(Directory.GetFiles(savestatesFolder, "*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension));
            allSavestates.Sort();
            curSelection = allSavestates;
        }
        catch (Exception e)
        {
            DebugMod.Instance.LogError($"Exception while loading savestates from {savestatesFolder}:\n{e}");
        }
    }

    public void StartSelectState(bool duped)
    {
        menuState = menuState == MenuState.None
            ? (duped ? MenuState.LoadStateDuped : MenuState.LoadState)
            : MenuState.None;
        DebugMod.Instance.LogDebug($"Current savestate menu state: {menuState}");
        if (menuState == MenuState.None)
            return;

        if (GameManager.instance.IsGamePaused())
        {
            DebugModRunner.Instance.AcceptingInput = false;
            LoadSavestateNames();
            GameManager.instance.inputHandler.acceptingInput = false;
            EventSystem.current.sendNavigationEvents = false;
            isLoadStateMenuFirstFrame = true;
        } 
        else
        {
            if (!String.IsNullOrEmpty(lastSavestate))
                StartCoroutine(LoadSavestate(duped, lastSavestate));
            else
                DebugMod.Instance.LogWarn("Tried to quick-load savestate before savestate created or loaded");
            menuState = MenuState.None;
        }
    }

    private IEnumerator LoadSavestate(bool duped, string savestateName)
    {
        DebugMod.Instance.LogDebug($"Loading savestate at {savestateName}.json");
        Savestate s;
        try
        {
            s = JsonUtility.FromJson<Savestate>(
                File.ReadAllText(Path.Combine(savestatesFolder, savestateName + ".json")));
        }
        catch (Exception e)
        {
            DebugMod.Instance.LogError($"Exception while loading savestate {savestateName}:\n{e}");
            yield break;
        }
        
        GameManager.instance.ChangeToScene(DUMMY_SCENE_NAME, "", 0f);
        yield return new WaitUntil(() => GameManager.instance.GetSceneNameString() == DUMMY_SCENE_NAME);
        GameManager.instance.ResetSemiPersistentItems();
        yield return null;

        PlayerData.instance = s.gameData.playerData;
        PlayerData.instance.hazardRespawnLocation = s.hazardRespawn;

        GameManager.instance.playerData = PlayerData.instance;
        HeroController.instance.playerData = PlayerData.instance;
        HeroController.instance.geoCounter.playerData = PlayerData.instance;
        HeroController.instance.GetField<HeroController, HeroAnimationController>("animCtrl")
            .SetField("pd", PlayerData.instance);

        SceneData.instance = s.gameData.sceneData;
        GameManager.instance.sceneData = SceneData.instance;

        HeroController.instance.transform.position = s.heroPosition;
        
        GameManager.instance.cameraCtrl.SetField("isGameplayScene", true);
        
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
        HeroController.instance.geoCounter.geoTextMesh.text = PlayerData.instance.geo.ToString();
        HeroController.instance.SetMPCharge(PlayerData.instance.MPCharge);
        if (PlayerData.instance.MPCharge == 0)
            GameCameras.instance.soulOrbFSM.SendEvent("MP LOSE");

        yield return null;
        GameManager.instance.ChangeToScene(s.loadedScenes[0], "", 0.4f);
        yield return new WaitUntil(() => GameManager.instance.GetSceneNameString() == s.loadedScenes[0]);
        
        if (duped)
            for (int i = 1; i < s.loadedScenes.Length; i++)
                yield return USceneManager.LoadSceneAsync(s.loadedScenes[i]);

        GameManager.instance.cameraCtrl.SetMode(CameraController.CameraMode.FOLLOWING);

        var rb2d = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
        rb2d.isKinematic = s.isKinematized;
        rb2d.velocity = s.heroVelocity;
        rb2d.gravityScale = s.heroGravityScale;
        HeroController.instance.SetField("prevGravityScale", s.heroPreviousGravityScale);

        PlayerData.instance.isInvincible = s.isDgateInvuln;
        Util.GetHeroBox().SetActive(!s.isDgateInvuln);

        DebugMod.Instance.LogDebug("Finished loading savestate");
    }

    public void CreateSavestate()
    {
        DebugMod.Instance.LogDebug("Creating savestate");
        var rb2d = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
        Savestate s = new Savestate
        {
            loadedScenes = Enumerable.Range(0, USceneManager.sceneCount)
                .Select(i => USceneManager.GetSceneAt(i).name)
                .ToArray(),
            heroPosition = HeroController.instance.transform.position,
            hazardRespawn = PlayerData.instance.hazardRespawnLocation,
            gameData = new SaveGameData(PlayerData.instance, SceneData.instance),
            isKinematized = rb2d.isKinematic,
            heroVelocity = rb2d.velocity,
            heroGravityScale = rb2d.gravityScale,
            heroPreviousGravityScale = HeroController.instance.GetField<HeroController, float>("prevGravityScale"),
            isDgateInvuln = PlayerData.instance.isInvincible && !Util.GetHeroBox().activeSelf
        };

        string fileName = Path.Combine(savestatesFolder,
            $"__TEMP_STATE_{GameManager.instance.GetSceneNameString()}__{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
        for (int i = 0;; i++)
        {
            if (File.Exists($"{fileName}__{i}.json"))
                continue;
            fileName = $"{fileName}__{i}.json";
            break;
        }
        File.WriteAllText(fileName, JsonUtility.ToJson(s, true));
        DebugMod.Instance.LogInfo($"Created savestate at {fileName}");
        // remove ".json" from end of fileName
        lastSavestate = fileName.Substring(0, fileName.Length - 5);
    }

    private void UpdateCurSelection()
    {
        curSelection = allSavestates.FindAll(s => s.ToLowerInvariant().Contains(query.ToLowerInvariant()));
        selectedSavestateIndex = 0;
    }

    private void CancelMenuInput()
    {
        DebugMod.Instance.LogDebug("Closing savestate menu");
        DebugModRunner.Instance.AcceptingInput = true;
        UIManager.instance.UIClosePauseMenu();
        GameCameras.instance.ResumeCameraShake();
        GameManager.instance.actorSnapshotUnpaused.TransitionTo(0f);
        GameManager.instance.isPaused = false;
        GameManager.instance.ui.AudioGoToGameplay(0.2f);
        HeroController.instance.UnPause();
        Time.timeScale = 1f;
        menuState = MenuState.None;
        GameManager.instance.inputHandler.StartUIInput();
    }

    private enum MenuState
    {
        None, LoadState, LoadStateDuped
    }
}