using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    private float speed = 3.0f;
    private float interpolation = 10f;
    private Rigidbody _rb;
    private Animator _animator;
    private Vector3 _currentDirection = Vector3.zero;
    public Animator characterAnimator;

    private GameObject plane;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = characterAnimator != null ? characterAnimator : GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal");
        float deltaZ = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(deltaX, 0f, deltaZ);

        if (direction != Vector3.zero)
        {
            _currentDirection = Vector3.Slerp(_currentDirection, direction, Time.deltaTime * interpolation);
            transform.rotation = Quaternion.LookRotation(_currentDirection);
        }

        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);

        if (_animator != null)
        {
            _animator.SetFloat("MoveSpeed", direction.magnitude);
            _animator.SetBool("Grounded", true);
        }

        if (Input.GetMouseButtonDown(0)) // clic gauche pour marquer la case
        {
            Destroy(plane);
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 3f, Vector3.down);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit h in hits)
            {
                if (h.collider.attachedRigidbody != _rb)
                {
                    plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    Destroy(plane.GetComponent<MeshCollider>());
                    plane.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                    plane.GetComponent<Renderer>().material.color = Color.blue;
                    plane.transform.position = new Vector3(
                        Mathf.Round(transform.position.x),
                        h.point.y + 0.01f,
                        Mathf.Round(transform.position.z)
                    );
                    break;
                }
            }
        }

        if (transform.position.y < -20)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
