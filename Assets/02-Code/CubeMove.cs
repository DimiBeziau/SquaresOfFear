using UnityEngine;

public class CubeMove : MonoBehaviour
{
    public static int destroyedCubes = 0;
    public static int destroyedMistake = 0;
    public static int blackFallen = 0;
    public static int countAudio = 0;

    public enum CubeKind
    {
        Basic    = 1,
        Green    = 2,
        Black    = 3,
        Platform = 4
    }

    public CubeKind kind;

    private Rigidbody _rb;
    private bool destroyed = false;

    private bool isRolling = false;
    private float rollProgress = 0f;
    private float rollDuration = 0.5f;
    private Vector3 rollStartPos;
    private Quaternion rollStartRot;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null) _rb.isKinematic = true; // on contrôle le mouvement manuellement
    }

    void Update()
    {
        if (isRolling)
        {
            rollProgress += Time.deltaTime / rollDuration;
            rollProgress = Mathf.Clamp01(rollProgress);

            transform.position = Vector3.Lerp(rollStartPos, rollStartPos + Vector3.back, rollProgress);
            transform.rotation = Quaternion.Lerp(rollStartRot, rollStartRot * Quaternion.Euler(-90f, 0f, 0f), rollProgress);

            if (rollProgress >= 1f)
                isRolling = false;
        }

        if (!isRolling && _rb != null && _rb.isKinematic)
        {
            bool overPlatform = Physics.Raycast(transform.position, Vector3.down, 5f, LayerMask.GetMask("Default"));
            if (!overPlatform)
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
            }
        }

        if (transform.position.y < -20f)
        {
            if ((int)kind != 4)
                destroyedMistake++;
            Destroy(gameObject);
        }
    }

    public void cubeAdvance(float speed)
    {
        if (isRolling) return;
        isRolling = true;
        rollProgress = 0f;
        rollDuration = 1f / speed;
        rollStartPos = transform.position;
        rollStartRot = transform.rotation;
    }

    public void ReactToHit(bool success)
    {
        if (destroyed) return;
        destroyed = true;

        if (success) destroyedCubes++;
        else         destroyedMistake++;

        countAudio++;
        Destroy(gameObject);
    }
}
