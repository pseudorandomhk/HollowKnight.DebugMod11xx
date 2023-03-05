using UnityEngine;

namespace DebugMod;

public static class Util
{
    public static GameObject GetHeroBox() => HeroController.instance.transform.Find("HeroBox").gameObject;
}