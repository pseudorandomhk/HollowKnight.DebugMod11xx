using System;

namespace DebugMod;

[Serializable]
public class Settings
{
    public string __VERSION__ = "0.0.0.0";
    
    public string toggleInfo = "f1";
    public string toggleInfiniteSoul = "f3";
    public string toggleInfiniteHealth = "f2";
    public string toggleInvincibility = "f4";
    public string toggleVignette = "insert";
    public string toggleNoclip = "[0]";
    public string cameraFollow = "f8";
    public string cycleHitboxes = "[5]";

    public string increaseLoadExtension = "f5";
    public string decreaseLoadExtension = "f6";

    public string zoomIn = "page up";
    public string zoomOut = "page down";
    public string resetZoom = "home";

    public string increaseTimescale = "[";
    public string decreaseTimescale = "]";
    public string resetTimescale = "\\";

    public string saveSavestate = "[7]";
    public string loadSavestate = "[8]";
    public string loadSavestateDuped = "[9]";

    public string toggleCollision = "[6]";
    public string toggleDreamgateInvuln = "[4]";
}