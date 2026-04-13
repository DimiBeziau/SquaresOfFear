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
        Golden   = 2,
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
    private int queuedRolls = 0;
    private float queuedRollSpeed = 1f;

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
            {
                SnapToGrid();
                SnapToRightAngles();

                if (!HasSupportBelow())
                {
                    StartFalling();
                }
                else if (queuedRolls > 0)
                {
                    queuedRolls--;
                    StartRoll(queuedRollSpeed);
                }
                else
                {
                    isRolling = false;
                }
            }
        }

        if (!isRolling && _rb != null && _rb.isKinematic && !HasSupportBelow())
        {
            StartFalling();
        }

        if (transform.position.y < -20f)
        {
            if (kind == CubeKind.Black)
            {
                blackFallen++;
                destroyedMistake++;
            }
            else if (kind != CubeKind.Platform)
            {
                destroyedMistake++;
            }
            Destroy(gameObject);
        }
    }

    public void cubeAdvance(float speed, bool force = false, int steps = 1)
    {
        steps = Mathf.Max(1, steps);

        if (isRolling && !force) return;

        if (force)
        {
            SnapToGrid();
            SnapToRightAngles();
            isRolling = false;
            queuedRolls = 0;
        }

        queuedRolls = steps - 1;
        queuedRollSpeed = speed;
        StartRoll(speed);
    }

    private void StartRoll(float speed)
    {
        isRolling = true;
        rollProgress = 0f;
        rollDuration = 1f / speed;
        rollStartPos = transform.position;
        rollStartRot = transform.rotation;
    }

    private void StartFalling()
    {
        isRolling = false;
        queuedRolls = 0;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }

    private bool HasSupportBelow()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 1f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (hit.collider.GetComponent<CubeMove>() != null) continue;
            return true;
        }
        return false;
    }

    private void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y * 2f) / 2f,
            Mathf.Round(transform.position.z)
        );
    }

    private void SnapToRightAngles()
    {
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(
            Mathf.Round(euler.x / 90f) * 90f,
            Mathf.Round(euler.y / 90f) * 90f,
            Mathf.Round(euler.z / 90f) * 90f
        );
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
