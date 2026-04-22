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
                if (sfxBonus != null) _audio.PlayOneShot(sfxBonus);
            }
            else if (cube.kind == CubeMove.CubeKind.Black)
            {
                CreatingLevel level = FindFirstObjectByType<CreatingLevel>();
                if (level != null) level.PenaltyAdvance();
                cube.ReactToHit(false);
                if (sfxMalus != null) _audio.PlayOneShot(sfxMalus);
            }
            else
            {
                cube.ReactToHit(true);
                if (sfxClassic != null) _audio.PlayOneShot(sfxClassic);
            }
        }
    }
}
