using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public void Posit(int x)
    {
        transform.Translate((float)-x / 2 + 0.5f + transform.position.x, 0, 0);
    }
}
