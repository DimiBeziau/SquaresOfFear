using System.Collections;
using UnityEngine;

public class PlatformLength : MonoBehaviour
{
    public GameObject platformCube;

    public IEnumerator Enlargement()
    {
        yield return new WaitForSeconds(3);
        transform.localScale += new Vector3(0, 0, 1f);
        transform.Translate(0, 0, 0.5f);
    }

    public void Decrease()
    {
        transform.localScale -= new Vector3(0, 0, 1f);
        transform.Translate(0, 0, -0.5f);
        float z = transform.localScale.z;
        for (int i = 0; i < transform.localScale.x; i++)
            Instantiate(platformCube, new Vector3(i, 0.48f, z), Quaternion.identity);
    }

    public int PlatformWidth(int i)
    {
        int j = (int)transform.localScale.x;
        transform.position = new Vector3(((float)i - 1) / 2, transform.position.y, transform.position.z);
        transform.localScale = new Vector3(i, transform.localScale.y, transform.localScale.z);
        return j;
    }
}
