using System.Collections.Generic;
using UnityEngine;

public class MarkerTrigger : MonoBehaviour
{
    public GameObject spherePrefab;
    public List<GameObject> spawnedSpheres = new List<GameObject>();

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

            if (cube.kind == CubeMove.CubeKind.Golden && spherePrefab != null)
            {
                Vector3 spawnPos = new Vector3(
                    Mathf.Round(col.transform.position.x),
                    col.transform.position.y + 1f,
                    Mathf.Round(col.transform.position.z)
                );
                spawnedSpheres.Add(Instantiate(spherePrefab, spawnPos, Quaternion.identity));
                cube.ReactToHit(true);
            }
            else if (cube.kind == CubeMove.CubeKind.Black)
            {
                CreatingLevel level = FindObjectOfType<CreatingLevel>();
                if (level != null) level.PenaltyAdvance();
                cube.ReactToHit(false);
            }
            else
            {
                cube.ReactToHit(true);
            }
        }
    }
}
