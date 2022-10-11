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
}