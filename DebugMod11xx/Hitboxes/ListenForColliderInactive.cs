using UnityEngine;

namespace DebugMod.Hitboxes;

public class ListenForColliderInactive : MonoBehaviour
{
    public Collider2D c2d;
    public LineRenderer render;
        
    private void Update()
    {
        if (c2d == null)
        {
            Destroy(this);
            return;
        }

        render.enabled = c2d.isActiveAndEnabled;
    }
}