using UnityEngine;

public class MarkerTrigger : MonoBehaviour
{
    public void Activate(float width = 1f, float depth = 1f)
    {
        Vector3 halfExtents = new Vector3(width / 2f, 1f, depth / 2f);
        Collider[] hits = Physics.OverlapBox(transform.position, halfExtents);

        foreach (Collider col in hits)
        {
            CubeMove cube = col.GetComponent<CubeMove>();
            if (cube == null) continue;

            if (width == 1f && depth == 1f)
            {
                int cubeX = Mathf.RoundToInt(col.transform.position.x);
                int cubeZ = Mathf.RoundToInt(col.transform.position.z);
                int triggerX = Mathf.RoundToInt(transform.position.x);
                int triggerZ = Mathf.RoundToInt(transform.position.z);
                if (cubeX != triggerX || cubeZ != triggerZ) continue;
            }

            cube.ReactToHit(true);
        }
    }
}
