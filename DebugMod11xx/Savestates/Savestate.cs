using System;
using UnityEngine;

namespace DebugMod.Savestates;

[Serializable]
public class Savestate
{
    public string[] loadedScenes;
    public Vector3 heroPosition;
    public Vector3 hazardRespawn;
    public SaveGameData gameData;
    public bool isKinematized = false;
    public Vector2 heroVelocity = Vector2.zero;
    public float heroGravityScale = HeroController.instance.DEFAULT_GRAVITY;
    public float heroPreviousGravityScale = 0f;
}