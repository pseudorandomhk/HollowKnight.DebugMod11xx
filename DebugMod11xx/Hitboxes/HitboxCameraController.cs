using System;
using UnityEngine;

namespace DebugMod.Hitboxes;

public class HitboxCameraController : MonoBehaviour
{
    private Camera cam;
    private RenderTexture camTexture;
    private Rect textureRect;
    private HitboxVisibility visibility;
    
    private void Start()
    {
        DebugMod.Instance.LogDebug("Starting hitbox camera controller");

        cam = base.gameObject.AddComponent<Camera>();
        cam.rect = new Rect(0, 0, 1, 1);
        cam.backgroundColor = Color.clear;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.Depth;
        cam.targetTexture = camTexture;

        camTexture = new RenderTexture(Screen.width, Screen.height, 0);
        textureRect = new Rect(0, 0, Screen.width, Screen.height);
        
        visibility = HitboxVisibility.None;
        UpdateHitboxVisibility();
        
        DebugMod.Instance.LogDebug("Finished starting hitbox camera controller");
    }

    public void CycleHitboxVisibility()
    {
        visibility = visibility.Next();
        UpdateHitboxVisibility();
    }

    private void UpdateHitboxVisibility()
    {
        cam.cullingMask = visibility switch
        {
            HitboxVisibility.None => 0,
            HitboxVisibility.Basic => 1 << HitboxHelper.REG_LAYER,
            HitboxVisibility.All => (1 << HitboxHelper.REG_LAYER) | (1 << HitboxHelper.MISC_LAYER),
            _ => throw new ArgumentOutOfRangeException()
        };
        cam.enabled = visibility != HitboxVisibility.None;
    }

    private void LateUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.IsGameplayScene())
            return;

        Camera gameCam = GameManager.instance.cameraCtrl.cam;
        cam.transform.position = gameCam.transform.position;
        // it just works(tm)
        cam.projectionMatrix = gameCam.projectionMatrix;
    }

    private void OnGUI()
    {
        if (visibility == HitboxVisibility.None || Event.current?.type != EventType.Repaint ||
            GameManager.instance == null || !GameManager.instance.IsGameplayScene())
            return;

        if (!camTexture.IsCreated())
            camTexture.Create();

        int prevDepth = GUI.depth;
        GUI.depth = 0;
        cam.Render();
        GUI.DrawTexture(textureRect, camTexture, ScaleMode.ScaleAndCrop, true);
        GUI.depth = prevDepth;
    }
}