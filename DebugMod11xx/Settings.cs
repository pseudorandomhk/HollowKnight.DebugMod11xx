using System;
using UnityEngine;

namespace DebugMod;

[Serializable]
public class Settings
{
    public string toggleInfo;
    public string toggleInfiniteSoul;
    public string toggleInfiniteHealth;
    public string toggleInvincibility;
    public string toggleVignette;
    public string toggleNoclip;
    public string cameraFollow;
    public string cycleHitboxes;

    public string increaseLoadExtension;
    public string decreaseLoadExtension;

    public string zoomIn;
    public string zoomOut;
    public string resetZoom;

    public string increaseTimescale;
    public string decreaseTimescale;
    public string resetTimescale;

    public string saveSavestate;
    public string loadSavestate;
    public string loadSavestateDuped;

    public Settings()
    {
        toggleInfo = "f4";
        toggleInfiniteSoul = "b";
        toggleInfiniteHealth = "n";
        toggleInvincibility = "m";
        toggleVignette = "insert";
        toggleNoclip = "left shift";
        cameraFollow = "f8";
        cycleHitboxes = "h";
        increaseLoadExtension = "=";
        decreaseLoadExtension = "-";
        zoomIn = "page up";
        zoomOut = "page down";
        resetZoom = "home";
        increaseTimescale = ".";
        decreaseTimescale = ",";
        resetTimescale = "/";
        saveSavestate = "f1";
        loadSavestate = "f2";
        loadSavestateDuped = "f3";
    }
}