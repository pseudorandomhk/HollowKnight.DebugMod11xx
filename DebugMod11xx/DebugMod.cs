﻿using System;
using System.IO;
using Modding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

public class DebugMod : Mod
{
    public override string Version => GetType().Assembly.GetName().Version.ToString();

    public static DebugMod Instance { get; private set; }
    public static readonly GUIStyle Style = new()
    {
        normal = { textColor = Color.white },
        alignment = TextAnchor.UpperLeft,
        padding = new RectOffset(5, 5, 5, 5),
        wordWrap = false,
        clipping = TextClipping.Overflow,
    };

    private static readonly string settingsPath =
        Path.Combine(Application.persistentDataPath, "DebugMod11xx.GlobalSettings.json");

    internal Settings settings;
    public bool AcceptingInput { get; internal set; }

    public DebugMod() : base("DebugMod")
    {
        if (Instance != null)
        {
            LogWarn("Instantiated multiple times!");
            return;
        }

        Instance = this;

        if (File.Exists(settingsPath))
        {
            try
            {
                settings = JsonUtility.FromJson<Settings>(File.ReadAllText(settingsPath));
                LogInfo("Successfully loaded settings");
            }
            catch (Exception e)
            {
                LogError($"Exception while loading settings from {settingsPath}, resetting to default settings:\n{e}");
                ResetSettings();
            }
        }
        else
        {
            LogInfo($"Settings not found at {settingsPath}, creating default settings");
            ResetSettings();
        }

        GameObject go = new("Debug Mod Runner");
        try
        {
            go.AddComponent<DebugModRunner>();
        }
        catch (Exception e)
        {
            LogError($"Exception while initializing debug mod runner:\n{e}");
        }
        Object.DontDestroyOnLoad(go);
    }

    private void ResetSettings()
    {
        settings = new Settings();
        try
        {
            File.WriteAllText(settingsPath, JsonUtility.ToJson(settings, true));
            LogInfo($"Wrote settings to {settingsPath}");
        }
        catch (Exception e)
        {
            LogError($"Exception while writing settings to {settingsPath}:\n{e}");
        }
    }
}