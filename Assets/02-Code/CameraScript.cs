using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Vector3 followOffset = new Vector3(2f, 4f, -6f);
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float rotationSpeed = 6f;

    private float platformOffsetX;

    void Start()
    {
        if (playerTarget == null)
        {
            PlayerMove playerMove = FindFirstObjectByType<PlayerMove>();
            if (playerMove != null)
                playerTarget = playerMove.transform;
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null)
            return;

        Vector3 desiredPosition = playerTarget.position + followOffset + new Vector3(platformOffsetX, 0f, 0f);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Quaternion desiredRotation = Quaternion.LookRotation(playerTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    public void Posit(int x)
    {
        float playerX = playerTarget != null ? playerTarget.position.x : 0f;
        platformOffsetX = ((x - 1) / 2f) - playerX;
    }
}
