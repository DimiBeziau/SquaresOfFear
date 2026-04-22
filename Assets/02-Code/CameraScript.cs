using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public void Posit(int x)
    {
        transform.position = new Vector3((x - 1) / 2f, transform.position.y, transform.position.z);
    }
}
