using System;
using UnityEngine;

namespace DebugMod;

[Serializable]
public class Settings
{
    public string __VERSION__ = "0.0.0.0";
    
    public string toggleInfo = KeyCode.F1.ToString();
    public string toggleInfiniteSoul = KeyCode.F3.ToString();
    public string toggleInfiniteHealth = KeyCode.F2.ToString();
    public string toggleInvincibility = KeyCode.F4.ToString();
    public string toggleVignette = KeyCode.Insert.ToString();
    public string toggleNoclip = KeyCode.Keypad0.ToString();
    public string cameraFollow = KeyCode.F8.ToString();
    public string cycleHitboxes = KeyCode.Keypad5.ToString();

    public string increaseLoadExtension = KeyCode.F5.ToString();
    public string decreaseLoadExtension = KeyCode.F6.ToString();

    public string zoomIn = KeyCode.PageUp.ToString();
    public string zoomOut = KeyCode.PageDown.ToString();
    public string resetZoom = KeyCode.Home.ToString();

    public string increaseTimescale = KeyCode.LeftBracket.ToString();
    public string decreaseTimescale = KeyCode.RightBracket.ToString();
    public string resetTimescale = KeyCode.Backslash.ToString();

    public string saveSavestate = KeyCode.Keypad7.ToString();
    public string loadSavestate = KeyCode.Keypad8.ToString();
    public string loadSavestateDuped = KeyCode.Keypad9.ToString();

    public string toggleCollision = KeyCode.Keypad6.ToString();
    public string toggleDreamgateInvuln = KeyCode.Keypad4.ToString();
}