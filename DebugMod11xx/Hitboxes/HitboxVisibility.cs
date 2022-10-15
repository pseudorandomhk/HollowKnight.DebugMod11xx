using System;

namespace DebugMod.Hitboxes;

public enum HitboxVisibility
{
    None, Basic, All
}

public static class HitboxVisibilityExtensions
{
    public static HitboxVisibility Next(this HitboxVisibility v)
    {
        return v switch
        {
            HitboxVisibility.None => HitboxVisibility.Basic,
            HitboxVisibility.Basic => HitboxVisibility.All,
            HitboxVisibility.All => HitboxVisibility.None,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}