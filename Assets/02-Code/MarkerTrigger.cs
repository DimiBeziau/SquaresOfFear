using System.Collections.Generic;
using UnityEngine;

public class MarkerTrigger : MonoBehaviour
{
    public GameObject spherePrefab;
    public List<GameObject> spawnedSpheres = new List<GameObject>();

    [Header("Audio")]
    public AudioClip sfxBonus;
    public AudioClip sfxMalus;
    public AudioClip sfxClassic;
    private AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    public void Activate(float width = 1f, float depth = 1f)
    {
        Vector3 halfExtents = new Vector3(width / 2f, 1f, depth / 2f);
        Collider[] hits = Physics.OverlapBox(transform.position, halfExtents);
        int triggerX = Mathf.RoundToInt(transform.position.x);
        int triggerZ = Mathf.RoundToInt(transform.position.z);
        CubeMove targetCube = null;
        float bestDistance = float.MaxValue;

        foreach (Collider col in hits)
        {
            CubeMove cube = col.GetComponent<CubeMove>();
            if (cube == null) continue;

            if (width == 1f && depth == 1f)
            {
                int cubeX = Mathf.RoundToInt(col.transform.position.x);
                int cubeZ = Mathf.RoundToInt(col.transform.position.z);
                if (cubeX != triggerX || cubeZ != triggerZ) continue;
            }

            float distance = Vector3.SqrMagnitude(col.transform.position - transform.position);
            if (distance >= bestDistance) continue;

            bestDistance = distance;
            targetCube = cube;
        }

        if (targetCube == null) return;

        if (targetCube.kind == CubeMove.CubeKind.Golden && spherePrefab != null)
        {
            Vector3 spawnPos = new Vector3(
                Mathf.Round(targetCube.transform.position.x),
                targetCube.transform.position.y + 1f,
                Mathf.Round(targetCube.transform.position.z)
            );
            spawnedSpheres.Add(Instantiate(spherePrefab, spawnPos, Quaternion.identity));
            targetCube.ReactToHit(true);
            if (sfxBonus != null) _audio.PlayOneShot(sfxBonus);
        }
        else if (targetCube.kind == CubeMove.CubeKind.Black)
        {
            targetCube.ReactToHit(false);
            CreatingLevel level = FindFirstObjectByType<CreatingLevel>();
            if (level != null) level.PenaltyAdvance();
            if (sfxMalus != null) _audio.PlayOneShot(sfxMalus);
        }
        else
        {
            targetCube.ReactToHit(true);
            if (sfxClassic != null) _audio.PlayOneShot(sfxClassic);
        }
    }
}
