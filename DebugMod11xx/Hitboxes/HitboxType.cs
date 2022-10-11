using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.Hitboxes;

public enum HitboxType
{
    Knight,
    Enemy,
    Attack,
    Terrain,
    Trigger,
    Breakable,
    Gate,
    HazardRespawn,
    Other
}

public static class HitboxTypeExtensions
{
    private static readonly Dictionary<HitboxType, Color> colorinfo = new()
    {
        { HitboxType.Knight, Color.yellow },
        { HitboxType.Enemy, new Color(0.8f, 0, 0) },
        { HitboxType.Attack, Color.cyan },
        { HitboxType.Terrain, new Color(0, 0.8f, 0) },
        { HitboxType.Trigger, new Color(0.5f, 0.5f, 1f) },
        { HitboxType.Breakable, new Color(1f, 0.75f, 0.8f) },
        { HitboxType.Gate, new Color(0, 0, 0.5f) },
        { HitboxType.HazardRespawn, new Color(0.5f, 0, 0.5f) },
        { HitboxType.Other, new Color(0.8f, 0.6f, 0.4f) }
    };

    public static Color GetColor(this HitboxType t)
    {
        return colorinfo[t];
    }
}